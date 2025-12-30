using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Permissions.GetAllPermissions;

public sealed class GetAllPermissionsHandler
{
    private readonly LifeOSDbContext _context;

    public GetAllPermissionsHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<GetAllPermissionsResponse>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var permissions = await _context.Permissions
            .AsNoTracking()
            .Where(p => !p.IsDeleted)
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Type)
            .ToListAsync(cancellationToken);

        var grouped = permissions
            .GroupBy(p => p.Module)
            .Select(g => new PermissionModuleDto(
                g.Key,
                g.Select(p => new PermissionDto(
                    p.Id,
                    p.Name,
                    p.Description ?? string.Empty,
                    p.Type)).ToList()))
            .OrderBy(m => m.ModuleName)
            .ToList();

        var response = new GetAllPermissionsResponse(grouped);
        return ApiResultExtensions.Success(response, "İzinler başarıyla getirildi");
    }
}

