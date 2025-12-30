using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Books.GetBookById;

public static class GetBookByIdEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/books/{id}", async (
            Guid id,
            GetBookByIdHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(id, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetBookById")
        .WithTags("Books")
        .RequireAuthorization(Domain.Constants.Permissions.BooksRead)
        .Produces<ApiResult<GetBookByIdResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetBookByIdResponse>>(StatusCodes.Status404NotFound);
    }
}

