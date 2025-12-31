using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.GamePlatforms.UpdateGamePlatform;

public sealed class UpdateGamePlatformHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICacheService _cache;

    public UpdateGamePlatformHandler(LifeOSDbContext context, ICacheService cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ApiResult<object>> HandleAsync(
        UpdateGamePlatformCommand command,
        CancellationToken cancellationToken)
    {
        var platform = await _context.GamePlatforms
            .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);

        if (platform is null)
            return ApiResultExtensions.Failure("Oyun platformu bulunamadı");

        // Aynı isimde başka bir platform var mı kontrol et
        bool nameExists = await _context.GamePlatforms
            .AnyAsync(x => x.Id != command.Id && x.Name.ToUpper() == command.Name.ToUpper(), cancellationToken);

        if (nameExists)
            return ApiResultExtensions.Failure("Bu platform adı zaten kullanılıyor");

        platform.Update(command.Name);
        _context.GamePlatforms.Update(platform);
        await _context.SaveChangesAsync(cancellationToken);

        // Cache invalidation
        await _cache.Add(
            CacheKeys.GamePlatformListVersion(),
            Guid.NewGuid().ToString("N"),
            null,
            null);

        return ApiResultExtensions.Success("Oyun platformu başarıyla güncellendi");
    }
}

