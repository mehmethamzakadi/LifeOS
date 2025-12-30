namespace LifeOS.Application.Features.Categories.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description = null,
    Guid? ParentId = null);

