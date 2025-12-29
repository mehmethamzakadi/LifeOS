using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Attributes;

namespace LifeOS.Domain.Events.WalletTransactionEvents;

[StoreInOutbox]
public class WalletTransactionUpdatedEvent : DomainEvent
{
    public Guid WalletTransactionId { get; }
    public string Title { get; }
    public decimal Amount { get; }
    public override Guid AggregateId => WalletTransactionId;

    public WalletTransactionUpdatedEvent(Guid walletTransactionId, string title, decimal amount)
    {
        WalletTransactionId = walletTransactionId;
        Title = title;
        Amount = amount;
    }
}

