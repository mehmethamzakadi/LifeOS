namespace LifeOS.Application.Features.Categories.GetAllCategories;

public sealed record GetAllCategoriesResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentId);

