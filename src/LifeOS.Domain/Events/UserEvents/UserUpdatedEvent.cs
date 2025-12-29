using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Attributes;

namespace LifeOS.Domain.Events.UserEvents;

[StoreInOutbox]
public class UserUpdatedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }
    public override Guid AggregateId => UserId;

    public UserUpdatedEvent(Guid userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }
}