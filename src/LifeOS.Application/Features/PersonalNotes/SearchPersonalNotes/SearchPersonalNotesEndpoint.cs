using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.PersonalNotes.SearchPersonalNotes;

public static class SearchPersonalNotesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/personalnotes/search", async (
            DataGridRequest request,
            SearchPersonalNotesHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.ToResult();
        })
        .WithName("SearchPersonalNotes")
        .WithTags("PersonalNotes")
        .RequireAuthorization(Domain.Constants.Permissions.PersonalNotesViewAll)
        .Produces<ApiResult<PaginatedListResponse<SearchPersonalNotesResponse>>>(StatusCodes.Status200OK);
    }
}

