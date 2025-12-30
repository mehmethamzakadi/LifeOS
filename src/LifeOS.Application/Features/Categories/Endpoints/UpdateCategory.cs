using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
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
                return Results.BadRequest(new { Error = "ID mismatch" });

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var category = await context.Categories
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (category is null)
            {
                return Results.NotFound(new { Error = ResponseMessages.Category.NotFound });
            }

            // Başka bir kategoride aynı isim var mı kontrol et
            var normalizedName = request.Name.ToUpperInvariant();
            bool nameExists = await context.Categories
                .AnyAsync(x => x.NormalizedName == normalizedName && x.Id != request.Id, cancellationToken);

            if (nameExists)
            {
                return Results.BadRequest(new { Error = ResponseMessages.Category.AlreadyExists });
            }

            // Parent kontrolü
            if (request.ParentId.HasValue)
            {
                if (request.ParentId.Value == request.Id)
                {
                    return Results.BadRequest(new { Error = "Kategori kendi üst kategorisi olamaz." });
                }

                var parentExists = await context.Categories
                    .AnyAsync(x => x.Id == request.ParentId.Value && !x.IsDeleted, cancellationToken);

                if (!parentExists)
                {
                    return Results.BadRequest(new { Error = "Üst kategori bulunamadı." });
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
                            return Results.BadRequest(new { Error = "Döngüsel kategori referansı oluşturulamaz." });
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

            return Results.NoContent();
        })
        .WithName("UpdateCategory")
        .WithTags("Categories")
        .RequireAuthorization(Domain.Constants.Permissions.CategoriesUpdate)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}

