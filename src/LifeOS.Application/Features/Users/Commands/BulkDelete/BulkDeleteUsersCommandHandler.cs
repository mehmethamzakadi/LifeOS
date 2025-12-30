using LifeOS.Application.Abstractions;
using LifeOS.Domain.Common;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Commands.BulkDelete;

public class BulkDeleteUsersCommandHandler : IRequestHandler<BulkDeleteUsersCommand, BulkDeleteUsersResponse>
{
    private readonly LifeOSDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public BulkDeleteUsersCommandHandler(
        LifeOSDbContext context,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<BulkDeleteUsersResponse> Handle(BulkDeleteUsersCommand request, CancellationToken cancellationToken)
    {
        var response = new BulkDeleteUsersResponse();

        foreach (var userId in request.UserIds)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

                if (user == null)
                {
                    response.Errors.Add($"Kullanıcı bulunamadı: ID {userId}");
                    response.FailedCount++;
                    continue;
                }

                user.Delete();
                _context.Users.Update(user);
                response.DeletedCount++;
            }
            catch (Exception ex)
            {
                response.Errors.Add($"Kullanıcı silinirken hata oluştu (ID {userId}): {ex.Message}");
                response.FailedCount++;
            }
        }

        // Tüm değişiklikleri tek transaction'da kaydet
        if (response.DeletedCount > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return response;
    }
}
