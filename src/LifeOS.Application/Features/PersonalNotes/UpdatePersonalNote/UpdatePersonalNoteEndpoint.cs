using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.PersonalNotes.UpdatePersonalNote;

public static class UpdatePersonalNoteEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/personalnotes/{id}", async (
            Guid id,
            UpdatePersonalNoteCommand command,
            UpdatePersonalNoteHandler handler,
            IValidator<UpdatePersonalNoteCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var result = await handler.HandleAsync(id, command, cancellationToken);
            return result.ToResult();
        })
        .WithName("UpdatePersonalNote")
        .WithTags("PersonalNotes")
        .RequireAuthorization(Domain.Constants.Permissions.PersonalNotesUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

