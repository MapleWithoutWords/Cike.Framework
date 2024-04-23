using Cike.EventBus.Local.Enums;
using Cike.EventBus.Local.Expressions;
using System.Reflection;

namespace Cike.EventBus.LocalEvent;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class LocalEventHandlerAttribute : Attribute
{
    public int Order { get; set; }

    public bool IsCancel { get; set; }

    public int RetryCount { get; set; }
    public FailureLevelEnum FailureLevel { get; set; }

    public LocalEventHandlerAttribute(int order = 99)
    {
        Order = order;
    }

    internal Type InstanceType { get; set; }
    internal MethodInfo EventHandlerMethod { get; set; }
    internal TaskMethodInvokeDelegate MethodDelegate { get; set; }
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
