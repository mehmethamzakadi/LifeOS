using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Categories.Commands.Delete;

/// <summary>
/// Handler for deleting a category
/// </summary>
public sealed class DeleteCategoryCommandHandler(
    LifeOSDbContext context,
    ICacheService cacheService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteCategoryCommand, IResult>
{
    public async Task<IResult> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        if (category is null)
            return new ErrorResult(ResponseMessages.Category.NotFound);

        // Alt kategori kontrolü - eğer alt kategoriler varsa silinemez
        var hasChildren = await context.Categories
            .AnyAsync(x => x.ParentId == request.Id && !x.IsDeleted, cancellationToken);
        if (hasChildren)
            return new ErrorResult("Bu kategorinin alt kategorileri bulunmaktadır. Önce alt kategorileri silmeniz gerekmektedir.");

        category.Delete();
        context.Categories.Update(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await cacheService.Remove(CacheKeys.Category(category.Id));

        await cacheService.Add(
            CacheKeys.CategoryGridVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return new SuccessResult(ResponseMessages.Category.Deleted);
    }
}
