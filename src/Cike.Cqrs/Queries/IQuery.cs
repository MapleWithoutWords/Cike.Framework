using Cike.EventBus;

namespace Cike.Cqrs.Queries;

public interface IQuery<TResult> : IEvent<TResult>
{
}
