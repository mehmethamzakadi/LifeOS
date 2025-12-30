using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.DeleteCategory;

public sealed class DeleteCategoryHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public DeleteCategoryHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        if (category is null)
            return ApiResultExtensions.Failure(ResponseMessages.Category.NotFound);

        // Alt kategori kontrolü
        var hasChildren = await _context.Categories
            .AnyAsync(x => x.ParentId == id && !x.IsDeleted, cancellationToken);
        
        if (hasChildren)
            return ApiResultExtensions.Failure("Bu kategorinin alt kategorileri bulunmaktadır. Önce alt kategorileri silmeniz gerekmektedir.");

        category.Delete();
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.Remove(CacheKeys.Category(category.Id));

        await _cacheService.Add(
            CacheKeys.CategoryGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success(ResponseMessages.Category.Deleted);
    }
}

