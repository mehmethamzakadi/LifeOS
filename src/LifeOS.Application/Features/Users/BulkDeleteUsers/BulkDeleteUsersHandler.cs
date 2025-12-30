using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.BulkDeleteUsers;

public sealed class BulkDeleteUsersHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public BulkDeleteUsersHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<BulkDeleteUsersResponse>> HandleAsync(
        BulkDeleteUsersCommand command,
        CancellationToken cancellationToken)
    {
        var deletedCount = 0;
        var failedCount = 0;
        var errors = new List<string>();

        foreach (var userId in command.UserIds)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

                if (user == null)
                {
                    errors.Add($"Kullanıcı bulunamadı: ID {userId}");
                    failedCount++;
                    continue;
                }

                user.Delete();
                _context.Users.Update(user);
                deletedCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Kullanıcı silinirken hata oluştu (ID {userId}): {ex.Message}");
                failedCount++;
            }
        }

        var response = new BulkDeleteUsersResponse(deletedCount, failedCount, errors);

        if (deletedCount > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        var message = failedCount > 0 
            ? $"{deletedCount} kullanıcı silindi, {failedCount} kullanıcı silinemedi"
            : $"{deletedCount} kullanıcı başarıyla silindi";

        return ApiResultExtensions.Success(response, message);
    }
}

