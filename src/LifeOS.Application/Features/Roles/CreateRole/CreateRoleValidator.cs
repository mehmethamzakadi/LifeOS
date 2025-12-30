using FluentValidation;

namespace LifeOS.Application.Features.Roles.CreateRole;

public sealed class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Rol adı gereklidir")
            .MinimumLength(3).WithMessage("Rol adı en az 3 karakter olmalıdır")
            .MaximumLength(100).WithMessage("Rol adı en fazla 100 karakter olabilir");
    }
}

