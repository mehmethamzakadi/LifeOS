using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.Queries.GetById;

public sealed class GetCategoryByIdQueryHandler(
    LifeOSDbContext context,
    ICacheService cacheService) : IRequestHandler<GetByIdCategoryQuery, IDataResult<GetByIdCategoryResponse>>
{
    public async Task<IDataResult<GetByIdCategoryResponse>> Handle(GetByIdCategoryQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.Category(request.Id);
        var cacheValue = await cacheService.Get<GetByIdCategoryResponse>(cacheKey);
        if (cacheValue is not null)
            return new SuccessDataResult<GetByIdCategoryResponse>(cacheValue);

        var category = await context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (category is null)
            return new ErrorDataResult<GetByIdCategoryResponse>("Kategori bilgisi bulunamadı.");

        var response = new GetByIdCategoryResponse(category.Id, category.Name, category.Description, category.ParentId);

        await cacheService.Add(
            cacheKey,
            response,
            DateTimeOffset.UtcNow.Add(CacheDurations.Category),
            null);

        return new SuccessDataResult<GetByIdCategoryResponse>(response);
    }
}
