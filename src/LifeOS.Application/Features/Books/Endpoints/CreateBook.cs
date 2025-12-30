using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Security;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Books.Endpoints;

public static class CreateBook
{
    public sealed record Request(
        string Title,
        string Author,
        string? CoverUrl = null,
        int TotalPages = 0,
        int CurrentPage = 0,
        BookStatus Status = BookStatus.ToRead,
        int? Rating = null,
        DateTime? StartDate = null,
        DateTime? EndDate = null);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(b => b.Title)
                .NotEmpty().WithMessage("Kitap adı boş olmamalıdır!")
                .MinimumLength(2).WithMessage("Kitap adı en az 2 karakter olmalıdır!")
                .MaximumLength(200).WithMessage("Kitap adı en fazla 200 karakter olmalıdır!")
                .MustBePlainText("Kitap adı HTML veya script içeremez!");

            RuleFor(b => b.Author)
                .NotEmpty().WithMessage("Yazar adı boş olmamalıdır!")
                .MinimumLength(2).WithMessage("Yazar adı en az 2 karakter olmalıdır!")
                .MaximumLength(100).WithMessage("Yazar adı en fazla 100 karakter olmalıdır!")
                .MustBePlainText("Yazar adı HTML veya script içeremez!");

            RuleFor(b => b.CoverUrl)
                .MaximumLength(500).WithMessage("Kapak URL'si en fazla 500 karakter olabilir!")
                .When(b => !string.IsNullOrWhiteSpace(b.CoverUrl));

            RuleFor(b => b.TotalPages)
                .GreaterThanOrEqualTo(0).WithMessage("Toplam sayfa sayısı 0 veya daha büyük olmalıdır!");

            RuleFor(b => b.CurrentPage)
                .GreaterThanOrEqualTo(0).WithMessage("Mevcut sayfa sayısı 0 veya daha büyük olmalıdır!")
                .LessThanOrEqualTo(b => b.TotalPages).WithMessage("Mevcut sayfa sayısı toplam sayfa sayısından büyük olamaz!")
                .When(b => b.TotalPages > 0);

            RuleFor(b => b.Rating)
                .InclusiveBetween(1, 10).WithMessage("Değerlendirme 1 ile 10 arasında olmalıdır!")
                .When(b => b.Rating.HasValue);
        }
    }

    public sealed record Response(Guid Id);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/books", async (
            Request request,
            LifeOSDbContext context,
            ICacheService cache,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var book = Book.Create(
                request.Title,
                request.Author,
                request.CoverUrl,
                request.TotalPages,
                request.CurrentPage,
                request.Status,
                request.Rating,
                request.StartDate,
                request.EndDate);

            await context.Books.AddAsync(book, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // Cache invalidation
            await cache.Add(
                CacheKeys.BookGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return Results.Created($"/api/books/{book.Id}", new Response(book.Id));
        })
        .WithName("CreateBook")
        .WithTags("Books")
        .RequireAuthorization(Domain.Constants.Permissions.BooksCreate)
        .Produces<Response>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

