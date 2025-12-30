using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.Queries.GetAll;

/// <summary>
/// Handler for getting all active categories
/// </summary>
public sealed class GetAllCategoriesQueryHandler(LifeOSDbContext context)
    : IRequestHandler<GetAllListCategoriesQuery, List<CategoryListItemDto>>
{
    public async Task<List<CategoryListItemDto>> Handle(GetAllListCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await context.Categories
            .Where(c => !c.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return categories.Select(c => new CategoryListItemDto(c.Id, c.Name, c.Description, c.ParentId)).ToList();
    }
}
