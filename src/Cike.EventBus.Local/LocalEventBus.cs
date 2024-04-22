
using Cike.EventBus.Local;
using Microsoft.Extensions.DependencyInjection;

namespace Cike.EventBus.LocalEvent;

public class LocalEventBus(IServiceScopeFactory _serviceScopeFactory, LocalEventHandlerRelationContainer _localEventHandlerRelationContainer) : ILocalEventBus
{
    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        if (!_localEventHandlerRelationContainer.EventHanlderRelations.ContainsKey(@event.GetType()))
        {
            return;
        }

        var eventHanlderDto = _localEventHandlerRelationContainer.EventHanlderRelations[@event.GetType()];

        using var serviceScope = _serviceScopeFactory.CreateScope();
        foreach (var item in eventHanlderDto.Handlers)
        {
            var instance = serviceScope.ServiceProvider.GetService(item.InstanceType);

            object[] paramList = new object[item.ParameterTypes.Length];
            for (int i = 0; i < item.ParameterTypes.Length; i++)
            {
                if (item.ParameterTypes[i]==typeof(TEvent))
                {
                    paramList[i] = @event;
                }
                else if (item.ParameterTypes[i] == typeof(CancellationToken))
                {
                    paramList[i] = cancellationToken;
                }
                else
                {
                    paramList[i] = serviceScope.ServiceProvider.GetRequiredService(item.ParameterTypes[i]);
                }
            }
        }
    }
}
