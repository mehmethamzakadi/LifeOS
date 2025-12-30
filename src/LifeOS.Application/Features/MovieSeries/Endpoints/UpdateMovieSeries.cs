using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Application.Common.Security;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.MovieSeries.Endpoints;

public static class UpdateMovieSeries
{
    public sealed record Request(
        Guid Id,
        string Title,
        string? CoverUrl = null,
        MovieSeriesType Type = MovieSeriesType.Movie,
        MovieSeriesPlatform Platform = MovieSeriesPlatform.Netflix,
        int? CurrentSeason = null,
        int? CurrentEpisode = null,
        MovieSeriesStatus Status = MovieSeriesStatus.ToWatch,
        int? Rating = null,
        string? PersonalNote = null);

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(m => m.Id)
                .NotEmpty().WithMessage("Film/Dizi ID'si boş olamaz!");

            RuleFor(m => m.Title)
                .NotEmpty().WithMessage("Film/Dizi adı boş olmamalıdır!")
                .MinimumLength(2).WithMessage("Film/Dizi adı en az 2 karakter olmalıdır!")
                .MaximumLength(200).WithMessage("Film/Dizi adı en fazla 200 karakter olmalıdır!")
                .MustBePlainText("Film/Dizi adı HTML veya script içeremez!");

            RuleFor(m => m.CurrentSeason)
                .GreaterThan(0).WithMessage("Sezon numarası 0'dan büyük olmalıdır!")
                .When(m => m.CurrentSeason.HasValue);

            RuleFor(m => m.CurrentEpisode)
                .GreaterThan(0).WithMessage("Bölüm numarası 0'dan büyük olmalıdır!")
                .When(m => m.CurrentEpisode.HasValue);

            RuleFor(m => m.Rating)
                .InclusiveBetween(1, 10).WithMessage("Değerlendirme 1 ile 10 arasında olmalıdır!")
                .When(m => m.Rating.HasValue);

            RuleFor(m => m.PersonalNote)
                .MaximumLength(2000).WithMessage("Kişisel not en fazla 2000 karakter olabilir!")
                .When(m => !string.IsNullOrWhiteSpace(m.PersonalNote));
        }
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("api/movieseries/{id}", async (
            Guid id,
            Request request,
            LifeOSDbContext context,
            ICacheService cache,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            if (id != request.Id)
                return ApiResultExtensions.Failure("ID uyuşmazlığı").ToResult();

            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ApiResultExtensions.ValidationError(errors).ToResult();
            }

            var movieSeries = await context.MovieSeries
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (movieSeries is null)
            {
                return ApiResultExtensions.Failure(ResponseMessages.MovieSeries.NotFound).ToResult();
            }

            movieSeries.Update(
                request.Title,
                request.CoverUrl,
                request.Type,
                request.Platform,
                request.CurrentSeason,
                request.CurrentEpisode,
                request.Status,
                request.Rating,
                request.PersonalNote);

            context.MovieSeries.Update(movieSeries);
            await context.SaveChangesAsync(cancellationToken);

            // Cache invalidation
            await cache.Add(
                CacheKeys.MovieSeries(movieSeries.Id),
                new GetMovieSeriesById.Response(
                    movieSeries.Id,
                    movieSeries.Title,
                    movieSeries.CoverUrl,
                    movieSeries.Type,
                    movieSeries.Platform,
                    movieSeries.CurrentSeason,
                    movieSeries.CurrentEpisode,
                    movieSeries.Status,
                    movieSeries.Rating,
                    movieSeries.PersonalNote),
                DateTimeOffset.UtcNow.Add(CacheDurations.MovieSeries),
                null);

            await cache.Add(
                CacheKeys.MovieSeriesGridVersion(),
                Guid.NewGuid().ToString("N"),
                null,
                null);

            return ApiResultExtensions.Success(ResponseMessages.MovieSeries.Updated).ToResult();
        })
        .WithName("UpdateMovieSeries")
        .WithTags("MovieSeries")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesUpdate)
        .Produces<ApiResult<object>>(StatusCodes.Status200OK)
        .Produces<ApiResult<object>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<object>>(StatusCodes.Status404NotFound);
    }
}

