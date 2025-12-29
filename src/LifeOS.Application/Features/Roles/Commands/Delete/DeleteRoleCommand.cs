using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Roles.Commands.Delete;

public sealed record DeleteRoleCommand(Guid Id) : IRequest<IResult>;
