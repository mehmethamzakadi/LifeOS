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

namespace LifeOS.Application.Features.PersonalNotes.Endpoints;

public static class UpdatePersonalNote
{
    public sealed record Request(
        Guid Id,
        string Title,
        string Content,
        string? Category = null,
        bool IsPinned = false,
        string? Tags = null);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(p => p.Id)
                .NotEmpty().WithMessage("Not ID'si boş olamaz!");

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

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/personalnotes/{id}", async (
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

            var personalNote = await context.PersonalNotes
                .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

            if (personalNote is null)
            {
                return ApiResultExtensions.Failure(ResponseMessages.PersonalNote.NotFound).ToResult();
            }

            personalNote.Update(
                request.Title,
                request.Content,
                request.Category,
                request.IsPinned,
                request.Tags);

            context.PersonalNotes.Update(personalNote);
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

            return ApiResultExtensions.Success(ResponseMessages.PersonalNote.Updated).ToResult();
        })
        .WithName("UpdatePersonalNote")
        .WithTags("PersonalNotes")
        .RequireAuthorization(Domain.Constants.Permissions.PersonalNotesUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

