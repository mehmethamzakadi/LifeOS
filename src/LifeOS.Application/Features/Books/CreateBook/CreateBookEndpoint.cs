using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Books.CreateBook;

public static class CreateBookEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/books", async (
            CreateBookCommand command,
            CreateBookHandler handler,
            IValidator<CreateBookCommand> validator,
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
                $"/api/books/{response.Id}",
                ResponseMessages.Book.Created);
        })
        .WithName("CreateBook")
        .WithTags("Books")
        .RequireAuthorization(Domain.Constants.Permissions.BooksCreate)
        .Produces<ApiResult<CreateBookResponse>>(StatusCodes.Status201Created)
        .Produces<ApiResult<CreateBookResponse>>(StatusCodes.Status400BadRequest);
    }
}

