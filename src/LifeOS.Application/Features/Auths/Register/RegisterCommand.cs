using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Auths.Register;

public sealed record RegisterCommand(string UserName, string Email, string Password) : IRequest<IResult>;
