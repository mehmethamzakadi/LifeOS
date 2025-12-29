using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Attributes;

namespace LifeOS.Domain.Events.UserEvents;

[StoreInOutbox]
public class UserDeletedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }
    public override Guid AggregateId => UserId;

    public UserDeletedEvent(Guid userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }
}