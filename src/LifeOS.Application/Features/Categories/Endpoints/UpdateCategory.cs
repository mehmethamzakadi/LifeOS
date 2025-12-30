using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Common.Security;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.Endpoints;

public static class UpdateCategory
{
    public sealed record Request(
        Guid Id,
        string Name,
        string? Description = null,
        Guid? ParentId = null);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("Kategori ID'si boş olamaz!");

            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Kategori adı bilgisi boş olmamalıdır!")
                .MinimumLength(5).WithMessage("Kategori adı en az 5 karakter olmalıdır!")
                .MaximumLength(100).WithMessage("Kategori adı en fazla 100 karakter olmalıdır!")
                .MustBePlainText("Kategori adı HTML veya script içeremez!");

            RuleFor(c => c.Description)
                .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir!")
                .When(c => !string.IsNullOrWhiteSpace(c.Description));
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/categories/{id}", async (
            Guid id,
            Request request,
            LifeOSDbContext context,
            ICacheService cache,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            if (id != request.Id)
                return ApiResultExtensions.Failure("ID uyuşmazlığı").ToResult();

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var category = await context.Categories
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (category is null)
            {
                return ApiResultExtensions.Failure(ResponseMessages.Category.NotFound).ToResult();
            }

            // Başka bir kategoride aynı isim var mı kontrol et
            var normalizedName = request.Name.ToUpperInvariant();
            bool nameExists = await context.Categories
                .AnyAsync(x => x.NormalizedName == normalizedName && x.Id != request.Id, cancellationToken);

            if (nameExists)
            {
                return ApiResultExtensions.Failure(ResponseMessages.Category.AlreadyExists).ToResult();
            }

            // Parent kontrolü
            if (request.ParentId.HasValue)
            {
                if (request.ParentId.Value == request.Id)
                {
                    return ApiResultExtensions.Failure("Kategori kendi üst kategorisi olamaz.").ToResult();
                }

                var parentExists = await context.Categories
                    .AnyAsync(x => x.Id == request.ParentId.Value && !x.IsDeleted, cancellationToken);

                if (!parentExists)
                {
                    return ApiResultExtensions.Failure("Üst kategori bulunamadı.").ToResult();
                }

                // Döngüsel referans kontrolü
                var parentCategory = await context.Categories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == request.ParentId.Value && !x.IsDeleted, cancellationToken);
                if (parentCategory != null)
                {
                    var currentParentId = parentCategory.ParentId;
                    while (currentParentId.HasValue)
                    {
                        if (currentParentId.Value == request.Id)
                        {
                            return ApiResultExtensions.Failure("Döngüsel kategori referansı oluşturulamaz.").ToResult();
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
            await context.SaveChangesAsync(cancellationToken);

            // Cache invalidation
            await cache.Add(
                CacheKeys.Category(category.Id),
                new GetCategoryById.Response(category.Id, category.Name, category.Description, category.ParentId),
                DateTimeOffset.UtcNow.Add(CacheDurations.Category),
                null);

            await cache.Add(
                CacheKeys.CategoryGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return ApiResultExtensions.Success(ResponseMessages.Category.Updated).ToResult();
        })
        .WithName("UpdateCategory")
        .WithTags("Categories")
        .RequireAuthorization(Domain.Constants.Permissions.CategoriesUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

