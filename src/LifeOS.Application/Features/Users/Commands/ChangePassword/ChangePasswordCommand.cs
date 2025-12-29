using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Users.Commands.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword) : IRequest<IResult>;
