using LifeOS.Application.Abstractions;
using LifeOS.Domain.Common.Results;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Queries.GetCurrentUserProfile;

public sealed class GetCurrentUserProfileQueryHandler(
    ICurrentUserService currentUserService,
    LifeOSDbContext context) : IRequestHandler<GetCurrentUserProfileQuery, IDataResult<GetCurrentUserProfileResponse>>
{
    public async Task<IDataResult<GetCurrentUserProfileResponse>> Handle(GetCurrentUserProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return new ErrorDataResult<GetCurrentUserProfileResponse>("Kullanıcı kimliği bulunamadı.");
        }

        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        var user = await context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return new ErrorDataResult<GetCurrentUserProfileResponse>("Kullanıcı bulunamadı.");
        }

        var response = new GetCurrentUserProfileResponse(
            user.Id,
            user.UserName.Value,
            user.Email.Value,
            user.PhoneNumber,
            user.ProfilePictureUrl,
            user.EmailConfirmed,
            user.CreatedDate);

        return new SuccessDataResult<GetCurrentUserProfileResponse>(response);
    }
}
