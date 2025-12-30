using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.PersonalNotes.CreatePersonalNote;

public static class CreatePersonalNoteEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/personalnotes", async (
            CreatePersonalNoteCommand command,
            CreatePersonalNoteHandler handler,
            IValidator<CreatePersonalNoteCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var response = await handler.HandleAsync(command, cancellationToken);
            return ApiResultExtensions.CreatedResult(
                response,
                $"/api/personalnotes/{response.Id}",
                ResponseMessages.PersonalNote.Created);
        })
        .WithName("CreatePersonalNote")
        .WithTags("PersonalNotes")
        .RequireAuthorization(Domain.Constants.Permissions.PersonalNotesCreate)
        .Produces<ApiResult<CreatePersonalNoteResponse>>(StatusCodes.Status201Created)
        .Produces<ApiResult<CreatePersonalNoteResponse>>(StatusCodes.Status400BadRequest);
    }
}

