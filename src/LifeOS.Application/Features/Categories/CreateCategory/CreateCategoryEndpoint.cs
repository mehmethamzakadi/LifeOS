using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Categories.CreateCategory;

public static class CreateCategoryEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/categories", async (
            CreateCategoryCommand command,
            CreateCategoryHandler handler,
            IValidator<CreateCategoryCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            try
            {
                var response = await handler.HandleAsync(command, cancellationToken);
                return ApiResultExtensions.CreatedResult(
                    response,
                    $"/api/categories/{response.Id}",
                    ResponseMessages.Category.Created);
            }
            catch (InvalidOperationException ex)
            {
                return ApiResultExtensions.Failure<CreateCategoryResponse>(ex.Message).ToResult();
            }
        })
        .WithName("CreateCategory")
        .WithTags("Categories")
        .RequireAuthorization(Domain.Constants.Permissions.CategoriesCreate)
        .Produces<ApiResult<CreateCategoryResponse>>(StatusCodes.Status201Created)
        .Produces<ApiResult<CreateCategoryResponse>>(StatusCodes.Status400BadRequest);
    }
}

