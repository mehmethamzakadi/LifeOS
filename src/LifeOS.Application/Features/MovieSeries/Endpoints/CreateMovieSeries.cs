using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Security;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Enums;
using LifeOS.Persistence.Contexts;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.MovieSeries.Endpoints;

public static class CreateMovieSeries
{
    public sealed record Request(
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

    public sealed record Response(Guid Id);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/movieseries", async (
            Request request,
            LifeOSDbContext context,
            ICacheService cache,
            IValidator<Request> validator,
            CancellationToken cancellationToken) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(new { Errors = validationResult.Errors.Select(e => e.ErrorMessage) });
            }

            var movieSeries = LifeOS.Domain.Entities.MovieSeries.Create(
                request.Title,
                request.CoverUrl,
                request.Type,
                request.Platform,
                request.CurrentSeason,
                request.CurrentEpisode,
                request.Status,
                request.Rating,
                request.PersonalNote);

            await context.MovieSeries.AddAsync(movieSeries, cancellationToken);
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

            return Results.Created($"/api/movieseries/{movieSeries.Id}", new Response(movieSeries.Id));
        })
        .WithName("CreateMovieSeries")
        .WithTags("MovieSeries")
        .RequireAuthorization(Domain.Constants.Permissions.MovieSeriesCreate)
        .Produces<Response>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

