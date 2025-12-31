using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.MovieSeriesGenres.UpdateMovieSeriesGenre;

public static class UpdateMovieSeriesGenreEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/movie-series-genres/{id:guid}", async (
            Guid id,
            UpdateMovieSeriesGenreCommand command,
            UpdateMovieSeriesGenreHandler handler,
            IValidator<UpdateMovieSeriesGenreCommand> validator,
            CancellationToken cancellationToken) =>
        {
            command = command with { Id = id };

            var validationResult = await validator.ValidateAsync(command, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToResult();
        })
        .WithName("UpdateMovieSeriesGenre")
        .WithTags("MovieSeriesGenres")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesGenresUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest);
    }
}

