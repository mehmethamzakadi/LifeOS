using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.UpdateRole;

public sealed class UpdateRoleHandler
{
    private readonly LifeOSDbContext _context;

    public UpdateRoleHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        UpdateRoleCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return ApiResultExtensions.Failure("ID uyuşmazlığı");

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == command.Id && !r.IsDeleted, cancellationToken);

        if (role == null)
            return ApiResultExtensions.Failure(ResponseMessages.Role.NotFound);

        var normalizedName = command.Name.ToUpperInvariant();
        var existingRole = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.NormalizedName == normalizedName, cancellationToken);
        if (existingRole != null && existingRole.Id != command.Id)
            return ApiResultExtensions.Failure(ResponseMessages.Role.AlreadyExistsWithName(command.Name));

        role.Update(command.Name);
        _context.Roles.Update(role);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResultExtensions.Success(ResponseMessages.Role.Updated);
    }
}

