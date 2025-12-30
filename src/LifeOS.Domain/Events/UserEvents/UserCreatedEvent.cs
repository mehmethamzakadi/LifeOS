using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.UserEvents;

public class UserCreatedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string UserName { get; }
    public string Email { get; }
    public override Guid AggregateId => UserId;

    public UserCreatedEvent(Guid userId, string userName, string email)
    {
        UserId = userId;
        UserName = userName;
        Email = email;
    }
}