namespace Cike.Data.DataFilters;

public class CikeDataFilterOptions
{
    public Dictionary<Type, DataFilterState> DefaultStates { get; }

    public CikeDataFilterOptions()
    {
        DefaultStates = new Dictionary<Type, DataFilterState>();
    }
}
