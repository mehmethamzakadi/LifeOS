using AutoMapper;
using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.SearchUsers;

public sealed class SearchUsersHandler
{
    private readonly LifeOSDbContext _context;
    private readonly IMapper _mapper;

    public SearchUsersHandler(LifeOSDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ApiResult<PaginatedListResponse<SearchUsersResponse>>> HandleAsync(
        DataGridRequest request,
        CancellationToken cancellationToken)
    {
        var pagination = request.PaginatedRequest;
        var query = _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .AsNoTracking()
            .AsQueryable();
        query = query.ToDynamic(request.DynamicQuery);
        var usersDynamic = await query.ToPaginateAsync(
            pagination.PageIndex,
            pagination.PageSize,
            cancellationToken);

        PaginatedListResponse<SearchUsersResponse> response = _mapper.Map<PaginatedListResponse<SearchUsersResponse>>(usersDynamic);
        return ApiResultExtensions.Success(response, "Kullanıcılar başarıyla getirildi");
    }
}

