using LifeOS.Application.Common.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.CreateRole;

public sealed class CreateRoleHandler
{
    private readonly LifeOSDbContext _context;

    public CreateRoleHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<CreateRoleResponse> HandleAsync(
        CreateRoleCommand command,
        CancellationToken cancellationToken)
    {
        var normalizedName = command.Name.ToUpperInvariant();
        var checkRole = await _context.Roles
            .AnyAsync(r => r.NormalizedName == normalizedName, cancellationToken);
        if (checkRole)
            throw new InvalidOperationException(ResponseMessages.Role.AlreadyExists);

        var role = Role.Create(command.Name);
        await _context.Roles.AddAsync(role, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateRoleResponse(role.Id);
    }
}

