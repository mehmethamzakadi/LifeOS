using FluentValidation;

namespace LifeOS.Application.Features.Users.AssignRolesToUser;

public sealed class AssignRolesToUserValidator : AbstractValidator<AssignRolesToUserCommand>
{
    public AssignRolesToUserValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı ID'si gereklidir");

        RuleFor(x => x.RoleIds)
            .NotNull().WithMessage("Rol listesi gereklidir");
    }
}

