using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Books.UpdateBook;

public static class UpdateBookEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/books/{id}", async (
            Guid id,
            UpdateBookCommand command,
            UpdateBookHandler handler,
            IValidator<UpdateBookCommand> validator,
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
        .WithName("UpdateBook")
        .WithTags("Books")
        .RequireAuthorization(Domain.Constants.Permissions.BooksUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

