using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.ActivityLogs.Queries.GetPaginatedList;

public class GetPaginatedActivityLogsQuery : IRequest<PaginatedListResponse<GetPaginatedActivityLogsResponse>>
{
    public DataGridRequest Request { get; set; }

    public GetPaginatedActivityLogsQuery(DataGridRequest request)
    {
        Request = request;
    }
}
