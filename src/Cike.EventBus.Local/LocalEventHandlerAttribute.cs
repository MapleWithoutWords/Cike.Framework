namespace Cike.EventBus.Local;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LocalEventHandlerAttribute : Attribute
{
    public int Order { get; set; }

    public bool IsCancel { get; set; }

    public int RetryCount { get; set; }

    public FailureLevelEnum FailureLevel { get; set; } = FailureLevelEnum.Throw;

    public LocalEventHandlerAttribute(int order = 100)
    {
        Order = order;
    }

    internal Type InstanceType { get; set; } = null!;
    internal MethodInfo EventHandlerMethod { get; set; } = null!;
    internal TaskMethodInvokeDelegate MethodDelegate { get; set; } = null!;
    internal Type[] ParameterTypes { get; set; } = default!;
    internal Type EventType { get; set; } = default!;

    internal List<LocalEventHandlerAttribute> ComputeCancelList(List<LocalEventHandlerAttribute> cancelHandlers)
    {
        var startCancelOrder = FailureLevel switch
        {
            FailureLevelEnum.Throw => Order - 1,
            FailureLevelEnum.ThrowAndCancel => Order,
            _ => throw new NotImplementedException()
        };

        return cancelHandlers.Where(cancelHandler => cancelHandler.Order <= startCancelOrder).ToList();
    }
}
