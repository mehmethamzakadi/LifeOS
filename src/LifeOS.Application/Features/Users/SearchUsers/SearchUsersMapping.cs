using AutoMapper;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Users.SearchUsers;

/// <summary>
/// AutoMapper profile for SearchUsers feature.
/// This mapping is specific to SearchUsers slice and kept within the slice.
/// </summary>
public sealed class SearchUsersMapping : Profile
{
    public SearchUsersMapping()
    {
        CreateMap<UserRole, UserRoleResponse>()
            .ConstructUsing(src => new UserRoleResponse(
                src.RoleId,
                src.Role != null ? src.Role.Name : string.Empty
            ));

        CreateMap<User, SearchUsersResponse>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles ?? new List<UserRole>()))
            .ReverseMap()
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore());
        CreateMap<Paginate<User>, PaginatedListResponse<SearchUsersResponse>>().ReverseMap();
    }
}

