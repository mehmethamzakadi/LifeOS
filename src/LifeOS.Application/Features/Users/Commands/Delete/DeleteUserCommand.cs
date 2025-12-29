using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Users.Commands.Delete;

public sealed record DeleteUserCommand(Guid Id) : IRequest<IResult>;
