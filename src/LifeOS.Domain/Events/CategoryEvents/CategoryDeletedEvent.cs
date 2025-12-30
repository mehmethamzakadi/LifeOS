using LifeOS.Domain.Common;

namespace LifeOS.Domain.Events.CategoryEvents;

public class CategoryDeletedEvent : DomainEvent
{
    public Guid CategoryId { get; }
    public string Name { get; }
    public override Guid AggregateId => CategoryId;

    public CategoryDeletedEvent(Guid categoryId, string name)
    {
        CategoryId = categoryId;
        Name = name;
    }
}