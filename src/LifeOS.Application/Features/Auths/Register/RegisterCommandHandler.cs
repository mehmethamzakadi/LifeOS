using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Auths.Register;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, IResult>
{
    private readonly LifeOSDbContext _context;
    private readonly IUserDomainService _userDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(
        LifeOSDbContext context,
        IUserDomainService userDomainService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _userDomainService = userDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        User? existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.Value == request.Email, cancellationToken);
        if (existingUser is not null)
        {
            return new ErrorResult("Bu e-posta adresi zaten kullanılıyor!");
        }

        var user = User.Create(request.UserName, request.Email, string.Empty);

        var passwordResult = _userDomainService.SetPassword(user, request.Password);
        if (!passwordResult.Success)
            return passwordResult;

        await _context.Users.AddAsync(user, cancellationToken);

        // ✅ Read-only lookup - tracking'e gerek yok (performans için)
        var userRole = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.NormalizedName == UserRoles.User.ToUpperInvariant() && !r.IsDeleted, cancellationToken);
        if (userRole != null)
        {
            var roleResult = _userDomainService.AddToRole(user, userRole);
            if (!roleResult.Success)
                return roleResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new SuccessResult("Kayıt işlemi başarılı. Giriş yapabilirsiniz.");
    }
}
