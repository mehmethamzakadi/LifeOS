using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.UserEvents;

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