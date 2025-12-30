namespace LifeOS.Application.Features.Users.GetProfile;

public sealed record GetProfileResponse(
    Guid Id,
    string UserName,
    string Email,
    string? PhoneNumber,
    string? ProfilePictureUrl,
    bool EmailConfirmed,
    DateTime CreatedDate);

