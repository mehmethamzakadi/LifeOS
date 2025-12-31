using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.MovieSeriesGenres.CreateMovieSeriesGenre;

public static class CreateMovieSeriesGenreEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/movie-series-genres", async (
            CreateMovieSeriesGenreCommand command,
            CreateMovieSeriesGenreHandler handler,
            IValidator<CreateMovieSeriesGenreCommand> validator,
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
                    $"/api/movie-series-genres/{response.Id}",
                    "Film/Dizi türü başarıyla oluşturuldu");
            }
            catch (InvalidOperationException ex)
            {
                return ApiResultExtensions.Failure<CreateMovieSeriesGenreResponse>(ex.Message).ToResult();
            }
        })
        .WithName("CreateMovieSeriesGenre")
        .WithTags("MovieSeriesGenres")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesGenresCreate)
        .Produces<ApiResult<CreateMovieSeriesGenreResponse>>(StatusCodes.Status201Created)
        .Produces<ApiResult<CreateMovieSeriesGenreResponse>>(StatusCodes.Status400BadRequest);
    }
}

