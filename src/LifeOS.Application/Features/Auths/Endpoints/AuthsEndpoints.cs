using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Auths.Endpoints;

public static class AuthsEndpoints
{
    public static void MapAuthsEndpoints(this IEndpointRouteBuilder app)
    {
        Register.MapEndpoint(app);
        Login.MapEndpoint(app);
        Logout.MapEndpoint(app);
        RefreshToken.MapEndpoint(app);
        PasswordReset.MapEndpoint(app);
        PasswordVerify.MapEndpoint(app);
    }
}

