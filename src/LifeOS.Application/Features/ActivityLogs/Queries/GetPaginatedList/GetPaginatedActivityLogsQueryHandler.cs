using AutoMapper;
using LifeOS.Domain.Common.Dynamic;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Persistence.Contexts;
using LifeOS.Persistence.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.ActivityLogs.Queries.GetPaginatedList;

public class GetPaginatedActivityLogsQueryHandler : IRequestHandler<GetPaginatedActivityLogsQuery, PaginatedListResponse<GetPaginatedActivityLogsResponse>>
{
    private readonly LifeOSDbContext _context;
    private readonly IMapper _mapper;

    public GetPaginatedActivityLogsQueryHandler(
        LifeOSDbContext context,
        IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedListResponse<GetPaginatedActivityLogsResponse>> Handle(
        GetPaginatedActivityLogsQuery request,
        CancellationToken cancellationToken)
    {
        DynamicQuery dynamicQuery = request.Request.DynamicQuery ?? new DynamicQuery();

        List<Sort> sortDescriptors = dynamicQuery.Sort?.ToList() ?? new List<Sort>();
        if (sortDescriptors.Count == 0)
        {
            sortDescriptors.Add(new Sort(nameof(ActivityLog.Timestamp), "desc"));
        }

        dynamicQuery.Sort = sortDescriptors;

        // ✅ Read-only sorgu - tracking'e gerek yok (performans için)
        var query = _context.ActivityLogs
            .Include(a => a.User)
            .AsNoTracking()
            .AsQueryable();
        query = query.ToDynamic(dynamicQuery);
        var activityLogs = await query.ToPaginateAsync(
            request.Request.PaginatedRequest.PageIndex,
            request.Request.PaginatedRequest.PageSize,
            cancellationToken);

        PaginatedListResponse<GetPaginatedActivityLogsResponse> response = _mapper.Map<PaginatedListResponse<GetPaginatedActivityLogsResponse>>(activityLogs);
        return response;
    }
}
