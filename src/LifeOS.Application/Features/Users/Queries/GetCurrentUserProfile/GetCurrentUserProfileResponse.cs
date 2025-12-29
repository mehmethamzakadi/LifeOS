namespace LifeOS.Application.Features.Users.Queries.GetCurrentUserProfile;

public sealed record GetCurrentUserProfileResponse(
    Guid Id,
    string UserName,
    string Email,
    string? PhoneNumber,
    string? ProfilePictureUrl,
    bool EmailConfirmed,
    DateTime CreatedDate);
