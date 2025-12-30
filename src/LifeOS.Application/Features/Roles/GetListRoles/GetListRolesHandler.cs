using AutoMapper;
using LifeOS.Application.Common;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Roles.GetListRoles;

public sealed class GetListRolesHandler
{
    private readonly LifeOSDbContext _context;
    private readonly IMapper _mapper;

    public GetListRolesHandler(LifeOSDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResult<PaginatedListResponse<GetListRolesResponse>>> HandleAsync(
        PaginatedRequest pageRequest,
        CancellationToken cancellationToken)
    {
        var query = _context.Roles
            .AsNoTracking()
            .AsQueryable();
        var roles = await query.ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize, cancellationToken);

        PaginatedListResponse<GetListRolesResponse> response = _mapper.Map<PaginatedListResponse<GetListRolesResponse>>(roles);
        return ApiResultExtensions.Success(response, "Roller başarıyla getirildi");
    }
}

