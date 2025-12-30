using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.Categories.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Categories.Commands.Update;

public sealed class UpdateCategoryCommandHandler(
    LifeOSDbContext context,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateCategoryCommand, IResult>
{
    public async Task<IResult> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (category is null)
        {
            return new ErrorResult(ResponseMessages.Category.NotFound);
        }

        // Başka bir kategoride aynı isim var mı kontrol et (mevcut kategori hariç)
        var normalizedName = request.Name.ToUpperInvariant();
        bool nameExists = await context.Categories
            .AnyAsync(x => x.NormalizedName == normalizedName && x.Id != request.Id, cancellationToken);

        if (nameExists)
        {
            return new ErrorResult(ResponseMessages.Category.AlreadyExists);
        }

        // Parent kontrolü - eğer parentId verilmişse, parent'ın var olduğunu ve döngüsel referans olmadığını kontrol et
        if (request.ParentId.HasValue)
        {
            // Kendi kendisinin parent'ı olamaz (bu kontrol entity'de de var ama burada da kontrol ediyoruz)
            if (request.ParentId.Value == request.Id)
            {
                return new ErrorResult("Kategori kendi üst kategorisi olamaz.");
            }

            var parentExists = await context.Categories
                .AnyAsync(x => x.Id == request.ParentId.Value && !x.IsDeleted, cancellationToken);

            if (!parentExists)
            {
                return new ErrorResult("Üst kategori bulunamadı.");
            }

            // Döngüsel referans kontrolü: Parent'ın kendisi veya üst kategorileri bu kategoriyi içeriyor mu?
            var parentCategory = await context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ParentId.Value && !x.IsDeleted, cancellationToken);
            if (parentCategory != null)
            {
                // Parent'ın parent'larını kontrol et (basit döngü kontrolü)
                var currentParentId = parentCategory.ParentId;
                while (currentParentId.HasValue)
                {
                    if (currentParentId.Value == request.Id)
                    {
                        return new ErrorResult("Döngüsel kategori referansı oluşturulamaz.");
                    }
                    var currentParent = await context.Categories
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == currentParentId.Value && !x.IsDeleted, cancellationToken);
                    if (currentParent == null) break;
                    currentParentId = currentParent.ParentId;
                }
            }
        }

        category.Update(request.Name, request.Description, request.ParentId);
        context.Categories.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Add(
            CacheKeys.Category(category.Id),
            new GetByIdCategoryResponse(category.Id, category.Name, category.Description, category.ParentId),
            DateTimeOffset.UtcNow.Add(CacheDurations.Category),
            null);

        await cacheService.Add(
            CacheKeys.CategoryGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.Category.Updated);
    }
}
