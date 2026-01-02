using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LifeOS.Infrastructure.Authorization;

/// <summary>
/// Permission requirement'larını handle eden authorization handler
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(ILogger<PermissionAuthorizationHandler> logger)
    {
        _logger = logger;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // Kullanıcının permission claim'lerini kontrol et
        var allPermissionClaims = context.User.FindAll(c => c.Type == "permission").ToList();
        
        bool hasPermission = false;

        if (allPermissionClaims.Any())
        {
            // Her permission ayrı bir claim olarak eklenmişse (normal durum)
            hasPermission = allPermissionClaims.Any(c => c.Value == requirement.Permission);
            
            // Eğer bulunamadıysa, belki array olarak serialize edilmiş olabilir (JSON array string olarak)
            if (!hasPermission && allPermissionClaims.Count == 1)
            {
                var singleClaim = allPermissionClaims.First();
                try
                {
                    // JSON array olarak parse etmeyi dene
                    var permissionArray = JsonSerializer.Deserialize<string[]>(singleClaim.Value);
                    if (permissionArray != null)
                    {
                        hasPermission = permissionArray.Contains(requirement.Permission);
                    }
                }
                catch (JsonException)
                {
                    // JSON parse edilemedi, normal claim olarak devam et
                }
            }
        }

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "Permission check failed. Required: {RequiredPermission}, Found {Count} permission claims",
                requirement.Permission,
                allPermissionClaims.Count);
        }

        return Task.CompletedTask;
    }
}
