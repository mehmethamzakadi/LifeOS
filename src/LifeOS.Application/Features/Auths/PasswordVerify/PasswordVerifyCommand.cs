using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Auths.PasswordVerify;

public sealed record PasswordVerifyCommand(string ResetToken, string UserId) : IRequest<IDataResult<bool>>;
