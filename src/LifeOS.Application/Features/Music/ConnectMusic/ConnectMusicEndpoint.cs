using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Net.Http;

namespace LifeOS.Application.Features.Music.ConnectMusic;

public static class ConnectMusicEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/music/connect", async (
            ConnectMusicCommand command,
            ConnectMusicHandler handler,
            IValidator<ConnectMusicCommand> validator,
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
                var result = await handler.HandleAsync(command, cancellationToken);
                return result.ToResult();
            }
            catch (HttpRequestException ex)
            {
                return ApiResultExtensions.Failure<ConnectMusicResponse>(
                    $"Spotify API hatası: {ex.Message}").ToResult();
            }
            catch (InvalidOperationException ex)
            {
                return ApiResultExtensions.Failure<ConnectMusicResponse>(
                    $"İşlem hatası: {ex.Message}").ToResult();
            }
            catch (Exception ex)
            {
                return ApiResultExtensions.Failure<ConnectMusicResponse>(
                    $"Beklenmeyen bir hata oluştu: {ex.Message}").ToResult();
            }
        })
        .WithName("ConnectMusic")
        .WithTags("Music")
        .RequireAuthorization()
        .Produces<ApiResult<ConnectMusicResponse>>(StatusCodes.Status200OK)
        .Produces<ApiResult<ConnectMusicResponse>>(StatusCodes.Status400BadRequest)
        .Produces<ApiResult<ConnectMusicResponse>>(StatusCodes.Status401Unauthorized);
    }
}

