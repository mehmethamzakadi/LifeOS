namespace LifeOS.Application.Features.Users.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);

