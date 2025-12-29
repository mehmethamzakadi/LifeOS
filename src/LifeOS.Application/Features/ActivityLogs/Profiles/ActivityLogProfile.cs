using AutoMapper;
using LifeOS.Application.Features.ActivityLogs.Queries.GetPaginatedList;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.ActivityLogs.Profiles;

public sealed class ActivityLogProfile : Profile
{
    public ActivityLogProfile()
    {
        CreateMap<ActivityLog, GetPaginatedActivityLogsResponse>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));

        CreateMap<Paginate<ActivityLog>, PaginatedListResponse<GetPaginatedActivityLogsResponse>>();
    }
}
