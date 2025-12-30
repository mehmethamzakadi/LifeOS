using FluentValidation;

namespace LifeOS.Application.Features.Permissions.AssignPermissionsToRole;

public sealed class AssignPermissionsToRoleValidator : AbstractValidator<AssignPermissionsToRoleCommand>
{
    public AssignPermissionsToRoleValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Rol ID'si gereklidir");

        RuleFor(x => x.PermissionIds)
            .NotNull().WithMessage("Permission listesi gereklidir");
    }
}

