using LifeOS.Application.Common.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Music.AnalyzeArtist;

public static class AnalyzeArtistEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/music/analyze-artist", async (
            [FromQuery] string artistName,
            AnalyzeArtistHandler handler,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(artistName))
            {
                return Results.BadRequest(ApiResultExtensions.Failure<AnalyzeArtistResponse>("Sanatçı adı gereklidir."));
            }

            var query = new AnalyzeArtistQuery(artistName);
            var result = await handler.HandleAsync(query, cancellationToken);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("AnalyzeArtist")
        .WithTags("Music")
        .Produces<ApiResult<AnalyzeArtistResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<AnalyzeArtistResponse>>(StatusCodes.Status400BadRequest)
        .AllowAnonymous(); // Client Credentials Flow kullanıldığı için login gerekmez
    }
}

