using AutoMapper;
using LifeOS.Domain.Common.Dynamic;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LifeOS.Application.Features.ActivityLogs.Queries.GetPaginatedList;

public class GetPaginatedActivityLogsQueryHandler : IRequestHandler<GetPaginatedActivityLogsQuery, PaginatedListResponse<GetPaginatedActivityLogsResponse>>
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IMapper _mapper;

    public GetPaginatedActivityLogsQueryHandler(
        IActivityLogRepository activityLogRepository,
        IMapper mapper)
    {
        _activityLogRepository = activityLogRepository;
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
        Paginate<ActivityLog> activityLogs = await _activityLogRepository.GetPaginatedListByDynamicAsync(
            dynamic: dynamicQuery,
            index: request.Request.PaginatedRequest.PageIndex,
            size: request.Request.PaginatedRequest.PageSize,
            include: a => a.Include(a => a.User!),
            cancellationToken: cancellationToken
        );

        PaginatedListResponse<GetPaginatedActivityLogsResponse> response = _mapper.Map<PaginatedListResponse<GetPaginatedActivityLogsResponse>>(activityLogs);
        return response;
    }
}
