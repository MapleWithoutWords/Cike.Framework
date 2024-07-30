namespace Cike.Uow.Enums;

public enum UnitOfWorkCommitState
{
    Unknown = 0,

    Uncommitted,

    Committed,

    Rollbacked,
}
