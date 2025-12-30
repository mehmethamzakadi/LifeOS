using LifeOS.Application.Common.Constants;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.CreateUser;

public sealed class CreateUserHandler
{
    private readonly LifeOSDbContext _context;
    private readonly IUserDomainService _userDomainService;

    public CreateUserHandler(LifeOSDbContext context, IUserDomainService userDomainService)
    {
        _context = context;
        _userDomainService = userDomainService;
    }

    public async Task<CreateUserResponse> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        if (existingUser is not null)
            throw new InvalidOperationException(ResponseMessages.User.EmailAlreadyExists);

        var existingUserName = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == command.UserName, cancellationToken);
        if (existingUserName is not null)
            throw new InvalidOperationException(ResponseMessages.User.UsernameAlreadyExists);

        var user = User.Create(command.UserName, command.Email, string.Empty);

        var passwordResult = _userDomainService.SetPassword(user, command.Password);
        if (!passwordResult.Success)
            throw new InvalidOperationException(passwordResult.Message);

        await _context.Users.AddAsync(user, cancellationToken);

        var userRole = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.NormalizedName == UserRoles.User.ToUpperInvariant(), cancellationToken);
        if (userRole != null)
        {
            var roleResult = _userDomainService.AddToRole(user, userRole);
            if (!roleResult.Success)
                throw new InvalidOperationException(roleResult.Message);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateUserResponse(user.Id);
    }
}

