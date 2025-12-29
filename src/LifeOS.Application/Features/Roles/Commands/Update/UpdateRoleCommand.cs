using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Roles.Commands.Update;

public sealed record UpdateRoleCommand(Guid Id, string Name) : IRequest<IResult>;
