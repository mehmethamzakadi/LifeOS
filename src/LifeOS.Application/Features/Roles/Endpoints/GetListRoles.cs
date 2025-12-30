using AutoMapper;
using LifeOS.Application.Common;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Constants;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.Endpoints;

public static class GetListRoles
{
    public sealed record Response : BaseEntityResponse
    {
        public string Name { get; init; } = string.Empty;
    }

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("api/roles", async (
            [AsParameters] PaginatedRequest pageRequest,
            LifeOSDbContext context,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var query = context.Roles
                .AsNoTracking()
                .AsQueryable();
            var roles = await query.ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize, cancellationToken);

            PaginatedListResponse<Response> response = mapper.Map<PaginatedListResponse<Response>>(roles);
            return Results.Ok(response);
        })
        .WithName("GetListRoles")
        .WithTags("Roles")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.RolesViewAll)
        .Produces<PaginatedListResponse<Response>>(StatusCodes.Status200OK);
    }
}

