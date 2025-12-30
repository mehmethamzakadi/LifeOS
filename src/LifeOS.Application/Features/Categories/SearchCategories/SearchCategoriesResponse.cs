using LifeOS.Application.Common;

namespace LifeOS.Application.Features.Categories.SearchCategories;

public sealed record SearchCategoriesResponse : BaseEntityResponse
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Guid? ParentId { get; init; }
    public string? ParentName { get; init; }
}

