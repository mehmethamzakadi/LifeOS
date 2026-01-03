using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Music.DisconnectMusic;

public sealed class DisconnectMusicHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DisconnectMusicHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<DisconnectMusicResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure<DisconnectMusicResponse>("Yetkisiz erişim");
        }

        var connection = await _context.MusicConnections
            .FirstOrDefaultAsync(c => c.UserId == userId.Value && !c.IsDeleted, cancellationToken);

        if (connection == null)
        {
            return ApiResultExtensions.Failure<DisconnectMusicResponse>("Spotify bağlantısı bulunamadı");
        }

        connection.Delete();
        _context.MusicConnections.Update(connection);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResultExtensions.Success(
            new DisconnectMusicResponse(true, "Spotify bağlantısı başarıyla kesildi"),
            "Spotify bağlantısı başarıyla kesildi");
    }
}

