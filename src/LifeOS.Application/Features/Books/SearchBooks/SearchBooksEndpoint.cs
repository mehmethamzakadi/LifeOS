using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Books.SearchBooks;

public static class SearchBooksEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/books/search", async (
            DataGridRequest request,
            SearchBooksHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.ToResult();
        })
        .WithName("SearchBooks")
        .WithTags("Books")
        .RequireAuthorization(Domain.Constants.Permissions.BooksViewAll)
        .Produces<ApiResult<PaginatedListResponse<SearchBooksResponse>>>(StatusCodes.Status200OK);
    }
}

