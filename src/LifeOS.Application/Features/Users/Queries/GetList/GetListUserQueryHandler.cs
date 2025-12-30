using AutoMapper;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.Users.Queries.GetList;

public sealed class GetListUserQueryHandler(LifeOSDbContext context, IMapper mapper) : IRequestHandler<GetListUsersQuery, PaginatedListResponse<GetListUserResponse>>
{
    public async Task<PaginatedListResponse<GetListUserResponse>> Handle(GetListUsersQuery request, CancellationToken cancellationToken)
    {
        var query = context.Users.AsNoTracking().AsQueryable();
        var userList = await query.ToPaginateAsync(request.PageRequest.PageIndex, request.PageRequest.PageSize, cancellationToken);

        // ✅ AutoMapper ile DTO'ya map ediyoruz
        PaginatedListResponse<GetListUserResponse> response = mapper.Map<PaginatedListResponse<GetListUserResponse>>(userList);
        return response;
    }
}
