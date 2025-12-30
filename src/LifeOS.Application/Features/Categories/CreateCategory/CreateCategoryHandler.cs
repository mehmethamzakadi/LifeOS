using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.Categories.GetCategoryById;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.CreateCategory;

public sealed class CreateCategoryHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public CreateCategoryHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<CreateCategoryResponse> HandleAsync(
        CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        // NormalizedName ile case-insensitive kontrol
        var normalizedName = command.Name.ToUpperInvariant();
        bool categoryExists = await _context.Categories
            .AnyAsync(x => x.NormalizedName == normalizedName, cancellationToken);

        if (categoryExists)
        {
            throw new InvalidOperationException("Bu kategori adı zaten mevcut!");
        }

        // Parent kontrolü
        if (command.ParentId.HasValue)
        {
            var parentExists = await _context.Categories
                .AnyAsync(x => x.Id == command.ParentId.Value && !x.IsDeleted, cancellationToken);

            if (!parentExists)
            {
                throw new InvalidOperationException("Üst kategori bulunamadı.");
            }
        }

        var category = Category.Create(command.Name, command.Description, command.ParentId);
        await _context.Categories.AddAsync(category, cancellationToken);
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

        return new CreateCategoryResponse(category.Id);
    }
}

