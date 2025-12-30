using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Features.Categories.Queries.GetById;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Categories.Commands.Create;

public sealed class CreateCategoryCommandHandler(
    LifeOSDbContext context,
    ICacheService cache,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryCommand, IResult>
{
    public async Task<IResult> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        // NormalizedName ile case-insensitive kontrol (database index kullanarak)
        var normalizedName = request.Name.ToUpperInvariant();
        bool categoryExists = await context.Categories
            .AnyAsync(x => x.NormalizedName == normalizedName, cancellationToken);

        if (categoryExists)
        {
            return new ErrorResult(ResponseMessages.Category.AlreadyExists);
        }

        // Parent kontrolü - eğer parentId verilmişse, parent'ın var olduğunu kontrol et
        if (request.ParentId.HasValue)
        {
            var parentExists = await context.Categories
                .AnyAsync(x => x.Id == request.ParentId.Value && !x.IsDeleted, cancellationToken);

            if (!parentExists)
            {
                return new ErrorResult("Üst kategori bulunamadı.");
            }
        }

        var category = Category.Create(request.Name, request.Description, request.ParentId);
        await context.Categories.AddAsync(category, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cache.Add(
            CacheKeys.Category(category.Id),
            new GetByIdCategoryResponse(Id: category.Id, Name: category.Name, Description: category.Description, ParentId: category.ParentId),
            DateTimeOffset.UtcNow.Add(CacheDurations.Category),
            null);

        await cache.Add(
            CacheKeys.CategoryGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.Category.Created);
    }
}
