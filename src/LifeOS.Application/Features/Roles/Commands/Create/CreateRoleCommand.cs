using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Roles.Commands.Create;

public sealed record CreateRoleCommand(string Name) : IRequest<IResult>;
