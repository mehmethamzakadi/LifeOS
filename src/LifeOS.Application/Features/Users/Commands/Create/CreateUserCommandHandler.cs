
using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Results;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Services;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IResult = LifeOS.Domain.Common.Results.IResult;

namespace LifeOS.Application.Features.Users.Commands.Create;

public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, IResult>
{
    private readonly LifeOSDbContext _context;
    private readonly IUserDomainService _userDomainService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateUserCommandHandler(
        LifeOSDbContext context,
        IUserDomainService userDomainService,
        IUnitOfWork unitOfWork)
    {
        _context = context;
        _userDomainService = userDomainService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.Value == request.Email, cancellationToken);
        if (existingUser is not null)
            return new ErrorResult("Bu e-posta adresi zaten kullanılıyor!");

        var existingUserName = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName.Value == request.UserName, cancellationToken);
        if (existingUserName is not null)
            return new ErrorResult("Bu kullanıcı adı zaten kullanılıyor!");

        var user = User.Create(request.UserName, request.Email, string.Empty);

        var passwordResult = _userDomainService.SetPassword(user, request.Password);
        if (!passwordResult.Success)
            return passwordResult;

        await _context.Users.AddAsync(user, cancellationToken);

        // ✅ Read-only lookup - tracking'e gerek yok (performans için)
        var userRole = await _context.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.NormalizedName == UserRoles.User.ToUpperInvariant(), cancellationToken);
        if (userRole != null)
        {
            var roleResult = _userDomainService.AddToRole(user, userRole);
            if (!roleResult.Success)
                return roleResult;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SuccessResult("Kullanıcı bilgisi başarıyla eklendi.");
    }
}
