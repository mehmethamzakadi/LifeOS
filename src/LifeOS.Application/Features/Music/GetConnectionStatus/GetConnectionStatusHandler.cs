using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Music.GetConnectionStatus;

public sealed class GetConnectionStatusHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetConnectionStatusHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<GetConnectionStatusResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure<GetConnectionStatusResponse>("Yetkisiz erişim");
        }

        var connection = await _context.MusicConnections
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == userId.Value && !c.IsDeleted, cancellationToken);

        if (connection == null || !connection.IsActive)
        {
            return ApiResultExtensions.Success(
                new GetConnectionStatusResponse(false, null, null, null),
                "Spotify hesabı bağlı değil");
        }

        var isTokenExpired = connection.ExpiresAt <= DateTime.UtcNow;

        return ApiResultExtensions.Success(
            new GetConnectionStatusResponse(
                true,
                connection.SpotifyUserId,
                connection.SpotifyUserName,
                isTokenExpired),
            "Bağlantı durumu getirildi");
    }
}

