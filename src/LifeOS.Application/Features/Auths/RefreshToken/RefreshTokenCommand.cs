using LifeOS.Application.Features.Auths.Login;
using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Auths.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : IRequest<IDataResult<LoginResponse>>;
