namespace Cike.EventBus.Local;

public class LocalEventHandlerRelationContainer
{
    public Dictionary<Type, LocalEventHandlerDto> EventHandlerRelations { get; } = new();
    public HashSet<Type> EventHandlerClassTypeList { get; } = new();

    public LocalEventHandlerRelationContainer(CikeModuleContainer cikeModuleContainer)
    {
        Build(cikeModuleContainer);
    }

    private void Build(CikeModuleContainer cikeModuleContainer)
    {
        cikeModuleContainer.ModuleTypes
            .SelectMany(e => e.Assembly.GetTypes())
            .Where(e => e.IsClass && !e.IsAbstract)
            .ToList()
            .ForEach(classType =>
        {
            foreach (var item in classType.GetMethods())
            {
                var attribute = item.GetCustomAttribute<LocalEventHandlerAttribute>();
                if (attribute == null)
                {
                    continue;
                }
                var parameters = item.GetParameters().Where(e => typeof(IEvent).IsAssignableFrom(e.ParameterType)).ToList();
                if (parameters.Count != 1)
                {
                    throw new ArgumentException($"Method '{classType.FullName}.{item.Name}' must have only one parameter of type LocalEvent.");
                }
                if (!EventHandlerClassTypeList.Contains(classType))
                {
                    EventHandlerClassTypeList.Add(classType);
                }
                var eventParameter = parameters.First();

                if (!EventHandlerRelations.ContainsKey(eventParameter.ParameterType))
                {
                    EventHandlerRelations[eventParameter.ParameterType] = new LocalEventHandlerDto();
                }

                attribute.InstanceType = classType;
                attribute.EventHandlerMethod = item;
                attribute.ParameterTypes = item.GetParameters().Select(e => e.ParameterType).ToArray();
                attribute.MethodDelegate = MethodInvokeBuilder.Build(item, classType);
                attribute.EventType = eventParameter.ParameterType;

                if (attribute.IsCancel)
                {
                    EventHandlerRelations[eventParameter.ParameterType].CancelHandlers.Add(attribute);
                }
                else
                {
                    EventHandlerRelations[eventParameter.ParameterType].Handlers.Add(attribute);
                }
            }
        });

        foreach (var item in EventHandlerRelations.Values)
        {
            item.Handlers = item.Handlers.OrderBy(e => e.Order).ToList();
            item.CancelHandlers = item.CancelHandlers.OrderBy(e => e.Order).ToList();
        }
    }
}

public class LocalEventHandlerDto
{
    public List<LocalEventHandlerAttribute> Handlers { get; set; } = new();
    public List<LocalEventHandlerAttribute> CancelHandlers { get; set; } = new();
}
