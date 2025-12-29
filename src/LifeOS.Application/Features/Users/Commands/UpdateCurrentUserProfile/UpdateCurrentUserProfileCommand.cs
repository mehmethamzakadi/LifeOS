using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Users.Commands.UpdateCurrentUserProfile;

public sealed record UpdateCurrentUserProfileCommand(
    string UserName,
    string Email,
    string? PhoneNumber,
    string? ProfilePictureUrl) : IRequest<IResult>;
