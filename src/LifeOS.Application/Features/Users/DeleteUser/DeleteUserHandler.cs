using LifeOS.Application.Common.Constants;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.DeleteUser;

public sealed class DeleteUserHandler
{
    private readonly LifeOSDbContext _context;

    public DeleteUserHandler(LifeOSDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResult<object>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

        if (user == null)
            return ApiResultExtensions.Failure(ResponseMessages.User.NotFound);

        user.Delete();
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        return ApiResultExtensions.Success(ResponseMessages.User.Deleted);
    }
}

