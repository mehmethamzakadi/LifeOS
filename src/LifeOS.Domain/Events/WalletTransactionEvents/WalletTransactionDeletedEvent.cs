using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Attributes;

namespace LifeOS.Domain.Events.WalletTransactionEvents;

[StoreInOutbox]
public class WalletTransactionDeletedEvent : DomainEvent
{
    public Guid WalletTransactionId { get; }
    public string Title { get; }
    public decimal Amount { get; }
    public override Guid AggregateId => WalletTransactionId;

    public WalletTransactionDeletedEvent(Guid walletTransactionId, string title, decimal amount)
    {
        WalletTransactionId = walletTransactionId;
        Title = title;
        Amount = amount;
    }
}

