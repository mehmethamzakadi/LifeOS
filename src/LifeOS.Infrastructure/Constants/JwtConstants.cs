namespace LifeOS.Infrastructure.Constants;

/// <summary>
/// JWT token yapılandırma sabitleri
/// </summary>
public static class JwtConstants
{
    /// <summary>
    /// JWT token validation için clock skew toleransı
    /// </summary>
    public static readonly TimeSpan ClockSkew = TimeSpan.FromSeconds(30);
}

