using AutoMapper;
using LifeOS.Domain.Common.Dynamic;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Queries.GetPaginatedListByDynamic;

public sealed class GetPaginatedListByDynamicUsersQueryHandler(
    LifeOSDbContext context,
    IMapper mapper) : IRequestHandler<GetPaginatedListByDynamicUsersQuery, PaginatedListResponse<GetPaginatedListByDynamicUsersResponse>>
{
    public async Task<PaginatedListResponse<GetPaginatedListByDynamicUsersResponse>> Handle(GetPaginatedListByDynamicUsersQuery request, CancellationToken cancellationToken)
    {
        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        var query = context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .AsQueryable();
        query = query.ToDynamic(request.DataGridRequest.DynamicQuery);
        var usersDynamic = await query.ToPaginateAsync(
            request.DataGridRequest.PaginatedRequest.PageIndex,
            request.DataGridRequest.PaginatedRequest.PageSize,
            cancellationToken);

        PaginatedListResponse<GetPaginatedListByDynamicUsersResponse> response = mapper.Map<PaginatedListResponse<GetPaginatedListByDynamicUsersResponse>>(usersDynamic);

        return response;
    }
}
