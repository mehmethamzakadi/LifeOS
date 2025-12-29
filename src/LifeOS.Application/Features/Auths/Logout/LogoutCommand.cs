using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Auths.Logout;

public sealed record LogoutCommand(string RefreshToken) : IRequest<IResult>;
