using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.PersonalNotes.GetPersonalNoteById;

public static class GetPersonalNoteByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/personalnotes/{id}", async (
            Guid id,
            GetPersonalNoteByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetPersonalNoteById")
        .WithTags("PersonalNotes")
        .RequireAuthorization(Domain.Constants.Permissions.PersonalNotesRead)
        .Produces<ApiResult<GetPersonalNoteByIdResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetPersonalNoteByIdResponse>>(StatusCodes.Status404NotFound);
    }
}

