using System.Linq.Expressions;
using System.Reflection;

namespace Cike.EventBus.Local.Expressions;

internal delegate Task TaskMethodInvokeDelegate(object target, params object?[] parameters);

internal delegate void VoidMethodInvokeDelegate(object target, object?[] parameters);

public class MethodInvokeBuilder
{
    internal static TaskMethodInvokeDelegate Build(MethodInfo methodInfo, Type targetType)
    {
        var targetParameter = Expression.Parameter(typeof(object), "target");
        var parametersParameter = Expression.Parameter(typeof(object?[]), "parameters");

        var parameters = new List<Expression>();
        var paramInfos = methodInfo.GetParameters();
        for (var i = 0; i < paramInfos.Length; i++)
        {
            var paramInfo = paramInfos[i];
            var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
            var valueCast = Expression.Convert(valueObj, paramInfo.ParameterType);

            parameters.Add(valueCast);
        }

        var instanceCast = Expression.Convert(targetParameter, targetType);
        var methodCall = Expression.Call(instanceCast, methodInfo, parameters);

        if (methodCall.Type == typeof(void))
        {
            var lambda = Expression.Lambda<VoidMethodInvokeDelegate>(methodCall, targetParameter, parametersParameter);
            var voidExecutor = lambda.Compile();
            return delegate (object target, object?[] parameters)
            {
                voidExecutor(target, parameters);
                return Task.CompletedTask;
            };
        }
        else if (methodCall.Type == typeof(Task))
        {
            var castMethodCall = Expression.Convert(methodCall, typeof(Task));
            var lambda = Expression.Lambda<TaskMethodInvokeDelegate>(castMethodCall, targetParameter, parametersParameter);
            return lambda.Compile();
        }
        else
        {
            throw new NotSupportedException($"The return type of the [{methodInfo.Name}] method must be Task or void");
        }
    }
}
