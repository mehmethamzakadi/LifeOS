namespace LifeOS.Application.Features.Users.UpdateProfile;

public sealed record UpdateProfileCommand(
    string UserName,
    string Email,
    string? PhoneNumber,
    string? ProfilePictureUrl);

