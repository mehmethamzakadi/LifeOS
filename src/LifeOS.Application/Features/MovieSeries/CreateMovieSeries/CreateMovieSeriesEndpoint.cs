using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.MovieSeries.CreateMovieSeries;

public static class CreateMovieSeriesEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/movieseries", async (
            CreateMovieSeriesCommand command,
            CreateMovieSeriesHandler handler,
            IValidator<CreateMovieSeriesCommand> validator,
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
                $"/api/movieseries/{response.Id}",
                ResponseMessages.MovieSeries.Created);
        })
        .WithName("CreateMovieSeries")
        .WithTags("MovieSeries")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesCreate)
        .Produces<ApiResult<CreateMovieSeriesResponse>>(StatusCodes.Status201Created)
        .Produces<ApiResult<CreateMovieSeriesResponse>>(StatusCodes.Status400BadRequest);
    }
}

