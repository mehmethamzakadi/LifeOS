using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.GetUserRoles;

public sealed class GetUserRolesHandler
{
    private readonly LifeOSDbContext _context;

    public GetUserRolesHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<GetUserRolesResponse>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

        if (user == null)
            return ApiResultExtensions.Failure<GetUserRolesResponse>("Kullanıcı bulunamadı");

        var userRoles = user.UserRoles
            .Where(ur => !ur.IsDeleted)
            .Select(ur => new UserRoleDto(ur.Role.Id, ur.Role.Name ?? string.Empty))
            .ToList();

        var response = new GetUserRolesResponse(
            user.Id,
            user.UserName,
            user.Email,
            userRoles);

        return ApiResultExtensions.Success(response, "Kullanıcı rolleri başarıyla getirildi");
    }
}

