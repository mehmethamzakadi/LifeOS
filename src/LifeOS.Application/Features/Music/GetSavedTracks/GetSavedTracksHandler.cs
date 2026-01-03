using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Music.GetSavedTracks;

public sealed class GetSavedTracksHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetSavedTracksHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<GetSavedTracksResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure<GetSavedTracksResponse>("Yetkisiz erişim");
        }

        var savedTracks = await _context.SavedTracks
            .Where(t => t.UserId == userId.Value && !t.IsDeleted)
            .OrderByDescending(t => t.SavedAt ?? t.CreatedDate)
            .Select(t => new SavedTrackDto(
                t.Id,
                t.UserId,
                t.SpotifyTrackId,
                t.Name,
                t.Artist,
                t.Album,
                t.AlbumCoverUrl,
                t.DurationMs,
                t.SavedAt,
                t.Notes,
                t.CreatedDate
            ))
            .ToListAsync(cancellationToken);

        return ApiResultExtensions.Success(
            new GetSavedTracksResponse(savedTracks),
            "Kaydedilmiş şarkılar getirildi");
    }
}

