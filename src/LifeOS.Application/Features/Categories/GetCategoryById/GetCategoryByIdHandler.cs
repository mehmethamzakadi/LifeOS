using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.GetCategoryById;

public sealed class GetCategoryByIdHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cacheService;

    public GetCategoryByIdHandler(LifeOSDbContext context, ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }

    public async Task<ApiResult<GetCategoryByIdResponse>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.Category(id);
        var cacheValue = await _cacheService.Get<GetCategoryByIdResponse>(cacheKey);
        if (cacheValue is not null)
            return ApiResultExtensions.Success(cacheValue, "Kategori bilgisi başarıyla getirildi");

        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (category is null)
            return ApiResultExtensions.Failure<GetCategoryByIdResponse>("Kategori bilgisi bulunamadı.");

        var response = new GetCategoryByIdResponse(
            category.Id,
            category.Name,
            category.Description,
            category.ParentId);

        await _cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.Category),
            null);

        return ApiResultExtensions.Success(response, "Kategori bilgisi başarıyla getirildi");
    }
}

