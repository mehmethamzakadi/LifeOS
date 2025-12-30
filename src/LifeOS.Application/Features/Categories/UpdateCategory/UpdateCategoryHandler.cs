using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Features.Categories.GetCategoryById;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.UpdateCategory;

public sealed class UpdateCategoryHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public UpdateCategoryHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        UpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return ApiResultExtensions.Failure("ID uyuşmazlığı");

        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (category is null)
        {
            return ApiResultExtensions.Failure(ResponseMessages.Category.NotFound);
        }

        // Başka bir kategoride aynı isim var mı kontrol et
        var normalizedName = command.Name.ToUpperInvariant();
        bool nameExists = await _context.Categories
            .AnyAsync(x => x.NormalizedName == normalizedName && x.Id != command.Id, cancellationToken);

        if (nameExists)
        {
            return ApiResultExtensions.Failure(ResponseMessages.Category.AlreadyExists);
        }

        // Parent kontrolü
        if (command.ParentId.HasValue)
        {
            if (command.ParentId.Value == command.Id)
            {
                return ApiResultExtensions.Failure("Kategori kendi üst kategorisi olamaz.");
            }

            var parentExists = await _context.Categories
                .AnyAsync(x => x.Id == command.ParentId.Value && !x.IsDeleted, cancellationToken);

            if (!parentExists)
            {
                return ApiResultExtensions.Failure("Üst kategori bulunamadı.");
            }

            // Döngüsel referans kontrolü
            var parentCategory = await _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == command.ParentId.Value && !x.IsDeleted, cancellationToken);
            if (parentCategory != null)
            {
                var currentParentId = parentCategory.ParentId;
                while (currentParentId.HasValue)
                {
                    if (currentParentId.Value == command.Id)
                    {
                        return ApiResultExtensions.Failure("Döngüsel kategori referansı oluşturulamaz.");
                    }
                    var currentParent = await _context.Categories
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == currentParentId.Value && !x.IsDeleted, cancellationToken);
                    if (currentParent == null) break;
                    currentParentId = currentParent.ParentId;
                }
            }
        }

        category.Update(command.Name, command.Description, command.ParentId);
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.Category(category.Id),
            new GetCategoryByIdResponse(category.Id, category.Name, category.Description, category.ParentId),
            DateTimeOffset.UtcNow.Add(CacheDurations.Category),
            null);

        await _cache.Add(
            CacheKeys.CategoryGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success(ResponseMessages.Category.Updated);
    }
}

