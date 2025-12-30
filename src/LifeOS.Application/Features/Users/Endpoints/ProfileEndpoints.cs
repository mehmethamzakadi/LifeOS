using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace LifeOS.Application.Features.Users.Endpoints;

public static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        GetProfile.MapEndpoint(app);
        UpdateProfile.MapEndpoint(app);
        ChangePassword.MapEndpoint(app);
    }
}

