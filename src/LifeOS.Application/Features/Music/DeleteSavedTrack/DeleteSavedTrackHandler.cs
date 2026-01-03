using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Music.DeleteSavedTrack;

public sealed class DeleteSavedTrackHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteSavedTrackHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid trackId,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure<object>("Yetkisiz erişim");
        }

        var savedTrack = await _context.SavedTracks
            .FirstOrDefaultAsync(t => t.Id == trackId && t.UserId == userId.Value && !t.IsDeleted, cancellationToken);

        if (savedTrack == null)
        {
            return ApiResultExtensions.Failure<object>("Kaydedilmiş şarkı bulunamadı");
        }

        savedTrack.Delete();
        _context.SavedTracks.Update(savedTrack);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResultExtensions.Success<object>(null, "Şarkı başarıyla silindi");
    }
}

