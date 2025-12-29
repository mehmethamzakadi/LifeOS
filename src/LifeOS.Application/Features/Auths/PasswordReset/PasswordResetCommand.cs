using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Auths.PasswordReset;

public sealed record PasswordResetCommand(string Email) : IRequest<IResult>;

