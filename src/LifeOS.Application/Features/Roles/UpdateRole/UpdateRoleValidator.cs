using FluentValidation;

namespace LifeOS.Application.Features.Roles.UpdateRole;

public sealed class UpdateRoleValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Rol ID'si gereklidir");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rol adı gereklidir")
            .MinimumLength(3).WithMessage("Rol adı en az 3 karakter olmalıdır")
            .MaximumLength(100).WithMessage("Rol adı en fazla 100 karakter olabilir");
    }
}

