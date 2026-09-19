
namespace Cike.EventBus.Local;

public class LocalEventExecutor(IServiceProvider _serviceProvider,
    IStrategyExecutor _strategyExecutor,
    ILocalEventContext _localEventContext,
    LocalEventHandlerRelationContainer _localEventHandlerRelationContainer,
    ILogger<LocalEventBus> _logger,
    ILocalEventMiddlewareProvider _eventMiddlewareProvider) : ILocalEventExecutor, IScopedDependency
{
    public async Task ExecuteAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
    {
        if (!_localEventHandlerRelationContainer.EventHandlerRelations.ContainsKey(@event.GetType()))
        {
            return;
        }

        var eventMiddlewares = _eventMiddlewareProvider.GetEventMiddlewares<TEvent>();

        EventHandlerDelegate eventHandlerDelegate = async () =>
        {
            await ExecuteHandlerAsync(@event, cancellationToken);
        };
        await eventMiddlewares.Reverse().Aggregate(eventHandlerDelegate, (next, middleware) => () => middleware.HandleAsync(@event, next))();
    }

    public async Task CancelAsync<TEvent>(TEvent @event, CancellationToken cancellationToken) where TEvent : IEvent
    {
        var eventHandlers = _localEventHandlerRelationContainer.EventHandlerRelations[@event.GetType()];

        var cancelHandlerResult = await ExecuteCancelHandlerAsync(eventHandlers.CancelHandlers, @event, cancellationToken);
        if (cancelHandlerResult.IsSucceed)
        {
            _localEventContext.Status = ExecutorStatusEnum.RollbackSucceeded;
        }
        else
        {
            ArgumentNullException.ThrowIfNull(_localEventContext.Exception);

            _localEventContext.Exception.Data.Add("Cancel handler exception", cancelHandlerResult.CancelException);
            _localEventContext.Status = ExecutorStatusEnum.RollbackFailed;
        }
    }

    private async Task ExecuteHandlerAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        var eventHanlderDto = _localEventHandlerRelationContainer.EventHandlerRelations[@event.GetType()];
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
                    _localEventContext.Exception = handlerException;

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
                            _localEventContext.Exception.Data.Add("cancel handler exception", cancelHandlerResult.Value.CancelException);
                            _localEventContext.Status = ExecutorStatusEnum.RollbackFailed;
                            break;
                        case { IsSucceed: true }:
                            _localEventContext.Status = ExecutorStatusEnum.RollbackSucceeded;
                            break;
                        default:
                            _localEventContext.Status = ExecutorStatusEnum.Failed;
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

            if (_localEventContext.Exception is not null)
                ExceptionDispatchInfo.Capture(_localEventContext.Exception).Throw();

            if (isCancel) return;
        }
    }

    async Task<(bool IsSucceed, Exception? CancelException)> ExecuteCancelHandlerAsync<TEvent>(
        IEnumerable<LocalEventHandlerAttribute> cancelHandlers,
        TEvent localEvent,
        CancellationToken cancellationToken) where TEvent : IEvent
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
        where TEvent : notnull
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
