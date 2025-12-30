using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.GetAllCategories;

public sealed class GetAllCategoriesHandler
{
    private readonly LifeOSDbContext _context;

    public GetAllCategoriesHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<List<GetAllCategoriesResponse>>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .Where(c => !c.IsDeleted)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var response = categories.Select(c => new GetAllCategoriesResponse(c.Id, c.Name, c.Description, c.ParentId)).ToList();

        return ApiResultExtensions.Success(response, "Kategoriler başarıyla getirildi");
    }
}

