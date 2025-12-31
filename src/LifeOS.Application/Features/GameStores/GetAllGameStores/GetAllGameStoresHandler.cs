using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.GameStores.GetAllGameStores;

public sealed class GetAllGameStoresHandler
{
    private readonly LifeOSDbContext _context;

    public GetAllGameStoresHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<List<GetAllGameStoresResponse>>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var stores = await _context.GameStores
            .Where(s => !s.IsDeleted)
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);

        var response = stores.Select(s => new GetAllGameStoresResponse(s.Id, s.Name)).ToList();

        return ApiResultExtensions.Success(response, "Oyun mağazaları başarıyla getirildi");
    }
}

