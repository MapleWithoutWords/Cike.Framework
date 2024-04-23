
using Cike.Core.DependencyInjection;
using Cike.EventBus.Local;
using Cike.EventBus.Local.Enums;
using Cike.EventBus.Local.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Cike.EventBus.LocalEvent;

public class LocalEventBus(IServiceProvider _serviceProvider,
    IStrategyExecutor _strategyExecutor,
    ILocalEventExecutor _localEventExecutor,
    LocalEventHandlerRelationContainer _localEventHandlerRelationContainer,
    ILogger<LocalEventBus> _logger) : ILocalEventBus, IScopedDependency
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        if (!_localEventHandlerRelationContainer.EventHanlderRelations.ContainsKey(@event.GetType()))
        {
            return;
        }

        var eventHanlderDto = _localEventHandlerRelationContainer.EventHanlderRelations[@event.GetType()];
        var isCancel = false;
        foreach (var item in eventHanlderDto.Handlers)
        {

            await _strategyExecutor.ExecuteAsync(item, @event, async () =>
            {
                _logger?.LogDebug("Publish event, event id: {EventId}, event: {@Event}", @event.GetEventId(), @event);
                var instance = _serviceProvider.GetRequiredService(item.InstanceType);
                var paramList = GetParameters(item, @event, cancellationToken);
                await item.MethodDelegate(instance, paramList);
              }, async (handlerException, failureLevel) =>
              {
                  if (failureLevel != FailureLevelEnum.Ignore)
                  {
                      isCancel = true;
                      _localEventExecutor.Exception = handlerException;

                      (bool IsSucceed, Exception? CancelException)? cancelHandlerResult = null;
                      if (eventHanlderDto.CancelHandlers.Any())
                      {
                          cancelHandlerResult = await ExecuteCancelHandlerAsync(
                              item.ComputeCancelList(eventHanlderDto.CancelHandlers),
                              @event,
                              cancellationToken);
                      }

                      switch (cancelHandlerResult)
                      {
                          case { IsSucceed: false }:
                              _localEventExecutor.Exception.Data.Add("cancel handler exception", cancelHandlerResult.Value.CancelException);
                              _localEventExecutor.Status = ExecutorStatusEnum.RollbackFailed;
                              break;
                          case { IsSucceed: true }:
                              _localEventExecutor.Status = ExecutorStatusEnum.RollbackSucceeded;
                              break;
                          default:
                              _localEventExecutor.Status = ExecutorStatusEnum.Failed;
                              break;
                      }
                  }
                  else
                  {
                      _logger?.LogError(
                          "Publishing event error is ignored, event id: {EventId}, instance: {InstanceName}, method: {MethodName}, event: {@Event}",
                          @event.GetEventId(),
                          item.InstanceType.FullName ?? item.InstanceType.Name,
                          item.EventHandlerMethod.Name,
                          @event);
                  }
              });

            if (_localEventExecutor.Exception is not null)
                ExceptionDispatchInfo.Capture(_localEventExecutor.Exception).Throw();

            if (isCancel) return;
        }
    }
    async Task<(bool IsSucceed, Exception? CancelException)> ExecuteCancelHandlerAsync<TEvent>(
    IEnumerable<LocalEventHandlerAttribute> cancelHandlers,
    TEvent localEvent,
    CancellationToken cancellationToken)
    where TEvent : IEvent
    {
        bool isSucceed = true;
        Exception? cancelException = null;
        foreach (var cancelHandler in cancelHandlers)
        {
            await _strategyExecutor.ExecuteAsync(cancelHandler, localEvent, async () =>
            {
                _logger?.LogDebug("Publish cancel event, event id: {EventId}, event: {@Event}", localEvent.GetEventId(), localEvent);
                var instance = _serviceProvider.GetRequiredService(cancelHandler.InstanceType);
                var paramList = GetParameters(cancelHandler, localEvent, cancellationToken);
                await cancelHandler.MethodDelegate(instance, paramList);
            }, (ex, failureLevel) =>
            {
                if (failureLevel != FailureLevelEnum.Ignore)
                {
                    isSucceed = false;
                    cancelException = ex;
                }

                _logger?.LogError("Publish cancel event ignored, event id: {EventId}, event: {@Event}", localEvent.GetEventId(), localEvent);

                return Task.CompletedTask;
            });

            if (cancelException is not null)
                return (isSucceed, cancelException);
        }
        return (isSucceed, cancelException);
    }

    private object[] GetParameters<TEvent>(LocalEventHandlerAttribute item, TEvent @event, CancellationToken cancellationToken = default)
    {

        object[] paramList = new object[item.ParameterTypes.Length];
        for (int i = 0; i < item.ParameterTypes.Length; i++)
        {
            if (item.ParameterTypes[i] == typeof(TEvent))
            {
                paramList[i] = @event;
            }
            else if (item.ParameterTypes[i] == typeof(CancellationToken))
            {
                paramList[i] = cancellationToken;
            }
            else
            {
                paramList[i] = _serviceProvider.GetRequiredService(item.ParameterTypes[i]);
            }
        }
        return paramList;
    }
}
