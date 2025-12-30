using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Security;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.PersonalNotes.Endpoints;

public static class CreatePersonalNote
{
    public sealed record Request(
        string Title,
        string Content,
        string? Category = null,
        bool IsPinned = false,
        string? Tags = null);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(p => p.Title)
                .NotEmpty().WithMessage("Not başlığı boş olmamalıdır!")
                .MinimumLength(2).WithMessage("Not başlığı en az 2 karakter olmalıdır!")
                .MaximumLength(200).WithMessage("Not başlığı en fazla 200 karakter olmalıdır!")
                .MustBePlainText("Not başlığı HTML veya script içeremez!");

            RuleFor(p => p.Content)
                .NotEmpty().WithMessage("Not içeriği boş olmamalıdır!");

            RuleFor(p => p.Category)
                .MaximumLength(100).WithMessage("Kategori en fazla 100 karakter olabilir!")
                .When(p => !string.IsNullOrWhiteSpace(p.Category));

            RuleFor(p => p.Tags)
                .MaximumLength(500).WithMessage("Etiketler en fazla 500 karakter olabilir!")
                .When(p => !string.IsNullOrWhiteSpace(p.Tags));
        }
    }

    public sealed record Response(Guid Id);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/personalnotes", async (
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

            var personalNote = PersonalNote.Create(
                request.Title,
                request.Content,
                request.Category,
                request.IsPinned,
                request.Tags);

            await context.PersonalNotes.AddAsync(personalNote, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            // Cache invalidation
            await cache.Add(
                CacheKeys.PersonalNote(personalNote.Id),
                new GetPersonalNoteById.Response(
                    personalNote.Id,
                    personalNote.Title,
                    personalNote.Content,
                    personalNote.Category,
                    personalNote.IsPinned,
                    personalNote.Tags),
                DateTimeOffset.UtcNow.Add(CacheDurations.PersonalNote),
                null);

            await cache.Add(
                CacheKeys.PersonalNoteGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return Results.Created($"/api/personalnotes/{personalNote.Id}", new Response(personalNote.Id));
        })
        .WithName("CreatePersonalNote")
        .WithTags("PersonalNotes")
        .RequireAuthorization(Domain.Constants.Permissions.PersonalNotesCreate)
        .Produces<Response>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

