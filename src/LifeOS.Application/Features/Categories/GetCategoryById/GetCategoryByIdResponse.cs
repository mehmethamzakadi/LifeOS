namespace LifeOS.Application.Features.Categories.GetCategoryById;

public sealed record GetCategoryByIdResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentId);

