namespace LifeOS.Application.Features.Auths.PasswordVerify;

public sealed record PasswordVerifyCommand(string ResetToken, string UserId);

