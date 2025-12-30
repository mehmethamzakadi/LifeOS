using FluentValidation;

namespace LifeOS.Application.Features.Users.BulkDeleteUsers;

public sealed class BulkDeleteUsersValidator : AbstractValidator<BulkDeleteUsersCommand>
{
    public BulkDeleteUsersValidator()
    {
        RuleFor(x => x.UserIds)
            .NotNull().WithMessage("Kullanıcı ID listesi gereklidir")
            .NotEmpty().WithMessage("En az bir kullanıcı ID'si gereklidir");
    }
}

