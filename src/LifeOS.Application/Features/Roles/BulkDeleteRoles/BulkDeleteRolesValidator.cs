using FluentValidation;

namespace LifeOS.Application.Features.Roles.BulkDeleteRoles;

public sealed class BulkDeleteRolesValidator : AbstractValidator<BulkDeleteRolesCommand>
{
    public BulkDeleteRolesValidator()
    {
        RuleFor(x => x.RoleIds)
            .NotNull().WithMessage("Rol ID listesi gereklidir")
            .NotEmpty().WithMessage("En az bir rol ID'si gereklidir");
    }
}

