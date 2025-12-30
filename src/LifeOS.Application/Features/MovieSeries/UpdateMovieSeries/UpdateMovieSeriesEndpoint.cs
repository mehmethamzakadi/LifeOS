using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.MovieSeries.UpdateMovieSeries;

public static class UpdateMovieSeriesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/movieseries/{id}", async (
            Guid id,
            UpdateMovieSeriesCommand command,
            UpdateMovieSeriesHandler handler,
            IValidator<UpdateMovieSeriesCommand> validator,
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
        .WithName("UpdateMovieSeries")
        .WithTags("MovieSeries")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

