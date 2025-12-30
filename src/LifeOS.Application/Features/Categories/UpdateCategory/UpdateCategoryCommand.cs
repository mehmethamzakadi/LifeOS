namespace LifeOS.Application.Features.Categories.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string? Description = null,
    Guid? ParentId = null);

