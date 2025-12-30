using AutoMapper;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Roles.GetListRoles;

/// <summary>
/// AutoMapper profile for GetListRoles feature.
/// This mapping is specific to GetListRoles slice and kept within the slice.
/// </summary>
public sealed class GetListRolesMapping : Profile
{
    public GetListRolesMapping()
    {
        CreateMap<Role, GetListRolesResponse>().ReverseMap();
        CreateMap<Paginate<Role>, PaginatedListResponse<GetListRolesResponse>>().ReverseMap();
    }
}

