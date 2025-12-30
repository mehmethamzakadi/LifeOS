using AutoMapper;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace LifeOS.Application.Features.Roles.Queries.GetList;

public sealed class GetListRoleQueryHandler(LifeOSDbContext context, IMapper mapper) : IRequestHandler<GetListRoleQuery, PaginatedListResponse<GetListRoleResponse>>
{

    public async Task<PaginatedListResponse<GetListRoleResponse>> Handle(GetListRoleQuery request, CancellationToken cancellationToken)
    {
        var query = context.Roles
            .AsNoTracking()
            .AsQueryable();
        var roles = await query.ToPaginateAsync(request.PageRequest.PageIndex, request.PageRequest.PageSize, cancellationToken);

        PaginatedListResponse<GetListRoleResponse> response = mapper.Map<PaginatedListResponse<GetListRoleResponse>>(roles);
        return response;
    }
}
