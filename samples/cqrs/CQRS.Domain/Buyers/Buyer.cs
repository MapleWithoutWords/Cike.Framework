using Cike.Data.Domain.Entities;

namespace CQRS.Domain.Buyers;

public class Buyer : FullAuditedAggregateRoot<long>
{
    public string Name { get; private set; } = default!;

    public int OrderCount { get; private set; }

    public decimal TotalAmount { get; private set; }

    private Buyer()
    {
    }

    public Buyer(long id, string name)
    {
        if (id <= 0) throw new UserFriendlyException("BuyerId must be greater than 0.");
        if (name.IsNullOrWhiteSpace()) throw new UserFriendlyException("Buyer name cannot be empty.");

        Id = id;
        Name = name;
    }

    public void RecordPlaced(decimal amount)
    {
        OrderCount++;
        TotalAmount += amount;
    }

    public void RecordCancelled(decimal amount)
    {
        OrderCount--;
        TotalAmount -= amount;
    }
}
