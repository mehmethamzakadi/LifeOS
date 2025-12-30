using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.GetProfile;

public sealed class GetProfileHandler
{
    private readonly LifeOSDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetProfileHandler(
        LifeOSDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResult<GetProfileResponse>> HandleAsync(
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetCurrentUserId();
        if (userId == null)
        {
            return ApiResultExtensions.Failure<GetProfileResponse>("Yetkisiz erişim");
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted, cancellationToken);

        if (user == null)
        {
            return ApiResultExtensions.Failure<GetProfileResponse>("Kullanıcı bulunamadı.");
        }

        var response = new GetProfileResponse(
            user.Id,
            user.UserName,
            user.Email,
            user.PhoneNumber,
            user.ProfilePictureUrl,
            user.EmailConfirmed,
            user.CreatedDate);

        return ApiResultExtensions.Success(response, "Profil bilgisi başarıyla getirildi");
    }
}

