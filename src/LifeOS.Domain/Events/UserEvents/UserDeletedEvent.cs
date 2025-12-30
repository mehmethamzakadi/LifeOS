using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.UserEvents;

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