using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Books.GetBookByIsbn;

public static class GetBookByIsbnEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/books/isbn/{isbn}", async (
            string isbn,
            [FromServices] GetBookByIsbnHandler handler,
            [FromServices] IValidator<GetBookByIsbnQuery> validator,
            CancellationToken cancellationToken) =>
        {
            var query = new GetBookByIsbnQuery(isbn);
            
            var validationResult = await validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var result = await handler.HandleAsync(query, cancellationToken);
            return result.ToResult();
        })
        .WithName("GetBookByIsbn")
        .WithTags("Books")
        .RequireAuthorization(Domain.Constants.Permissions.BooksRead)
        .Produces<ApiResult<GetBookByIsbnResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<GetBookByIsbnResponse>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<GetBookByIsbnResponse>>(StatusCodes.Status404NotFound);
    }
}

