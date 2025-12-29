using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Users.Commands.Create;

public sealed record CreateUserCommand(string UserName, string Email, string Password) : IRequest<IResult>;
