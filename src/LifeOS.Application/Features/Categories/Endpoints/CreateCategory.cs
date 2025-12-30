using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Common.Security;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Categories.Endpoints;

public static class CreateCategory
{
    public sealed record Request(
        string Name,
        string? Description = null,
        Guid? ParentId = null);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
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

    public sealed record Response(Guid Id);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/categories", async (
            Request request,
            LifeOSDbContext context,
            ICacheService cache,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            // NormalizedName ile case-insensitive kontrol
            var normalizedName = request.Name.ToUpperInvariant();
            bool categoryExists = await context.Categories
                .AnyAsync(x => x.NormalizedName == normalizedName, cancellationToken);

            if (categoryExists)
            {
                return ApiResultExtensions.Failure<Response>(ResponseMessages.Category.AlreadyExists).ToResult();
            }

            // Parent kontrolü
            if (request.ParentId.HasValue)
            {
                var parentExists = await context.Categories
                    .AnyAsync(x => x.Id == request.ParentId.Value && !x.IsDeleted, cancellationToken);

                if (!parentExists)
                {
                    return ApiResultExtensions.Failure<Response>("Üst kategori bulunamadı.").ToResult();
                }
            }

            var category = Category.Create(request.Name, request.Description, request.ParentId);
            await context.Categories.AddAsync(category, cancellationToken);
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

            var response = new Response(category.Id);
            return ApiResultExtensions.CreatedResult(response, $"/api/categories/{category.Id}", ResponseMessages.Category.Created);
        })
        .WithName("CreateCategory")
        .WithTags("Categories")
        .RequireAuthorization(Domain.Constants.Permissions.CategoriesCreate)
        .Produces<ApiResult<Response>>(StatusCodes.Status201Created)
        .Produces<ApiResult<Response>>(StatusCodes.Status400BadRequest);
    }
}

