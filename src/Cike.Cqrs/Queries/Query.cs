using Cike.EventBus;

namespace Cike.Cqrs.Queries;

public record Query<TResult> : Event<TResult>, IQuery<TResult>
{
}
