using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.CategoryEvents;

public class CategoryCreatedEvent : DomainEvent
{
    public Guid CategoryId { get; }
    public string Name { get; }
    public override Guid AggregateId => CategoryId;

    public CategoryCreatedEvent(Guid categoryId, string name)
    {
        CategoryId = categoryId;
        Name = name;
    }
}