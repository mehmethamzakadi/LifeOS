using LifeOS.Domain.Common;
using LifeOS.Domain.Enums;
using LifeOS.Domain.Events.WalletTransactionEvents;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Cüzdan işlem entity'si
/// </summary>
public sealed class WalletTransaction : BaseEntity
{
    // EF Core için parameterless constructor
    public WalletTransaction() { }

    public string Title { get; set; } = default!;
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionCategory Category { get; set; }
    public DateTime TransactionDate { get; set; }

    public static WalletTransaction Create(string title, decimal amount, TransactionType type, TransactionCategory category, DateTime transactionDate)
    {
        var walletTransaction = new WalletTransaction
        {
            Id = Guid.NewGuid(),
            Title = title,
            Amount = amount,
            Type = type,
            Category = category,
            TransactionDate = transactionDate,
            CreatedDate = DateTime.UtcNow
        };

        walletTransaction.AddDomainEvent(new WalletTransactionCreatedEvent(walletTransaction.Id, title, amount));
        return walletTransaction;
    }

    public void Update(string title, decimal amount, TransactionType type, TransactionCategory category, DateTime transactionDate)
    {
        Title = title;
        Amount = amount;
        Type = type;
        Category = category;
        TransactionDate = transactionDate;
        UpdatedDate = DateTime.UtcNow;

        AddDomainEvent(new WalletTransactionUpdatedEvent(Id, title, amount));
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
        AddDomainEvent(new WalletTransactionDeletedEvent(Id, Title, Amount));
    }
}

