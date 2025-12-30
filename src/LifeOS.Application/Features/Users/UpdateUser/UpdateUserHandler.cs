using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.UpdateUser;

public sealed class UpdateUserHandler
{
    private readonly LifeOSDbContext _context;

    public UpdateUserHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return ApiResultExtensions.Failure("ID uyuşmazlığı");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == command.Id && !u.IsDeleted, cancellationToken);
        if (user is null)
            return ApiResultExtensions.Failure(ResponseMessages.User.NotFound);

        if (user.Email != command.Email)
        {
            var existingEmail = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
            if (existingEmail != null && existingEmail.Id != command.Id)
                return ApiResultExtensions.Failure(ResponseMessages.User.EmailAlreadyExists);
        }

        if (user.UserName != command.UserName)
        {
            var existingUserName = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserName == command.UserName, cancellationToken);
            if (existingUserName != null && existingUserName.Id != command.Id)
                return ApiResultExtensions.Failure(ResponseMessages.User.UsernameAlreadyExists);
        }

        user.Update(command.UserName, command.Email);
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResultExtensions.Success(ResponseMessages.User.Updated);
    }
}

