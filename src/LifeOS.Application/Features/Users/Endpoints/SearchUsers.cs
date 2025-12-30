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

namespace LifeOS.Application.Features.Users.Endpoints;

public static class SearchUsers
{
    public sealed record Response : BaseEntityResponse
    {
        public string UserName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public IReadOnlyCollection<UserRoleResponse> Roles { get; init; } = Array.Empty<UserRoleResponse>();
    }

    public sealed record UserRoleResponse(Guid Id, string Name);

    public static void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("api/users/search", async (
            DataGridRequest request,
            LifeOSDbContext context,
            IMapper mapper,
            CancellationToken cancellationToken) =>
        {
            var pagination = request.PaginatedRequest;
            var query = context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .AsQueryable();
            query = query.ToDynamic(request.DynamicQuery);
            var usersDynamic = await query.ToPaginateAsync(
                pagination.PageIndex,
                pagination.PageSize,
                cancellationToken);

            PaginatedListResponse<Response> response = mapper.Map<PaginatedListResponse<Response>>(usersDynamic);
            return Results.Ok(response);
        })
        .WithName("SearchUsers")
        .WithTags("Users")
        .RequireAuthorization(LifeOS.Domain.Constants.Permissions.UsersViewAll)
        .Produces<PaginatedListResponse<Response>>(StatusCodes.Status200OK);
    }
}

