namespace LifeOS.Infrastructure.Options;

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordResetOptions";

    public string BaseUrl { get; set; } = string.Empty;
}
