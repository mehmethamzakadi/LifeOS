using AutoMapper;
using LifeOS.Application.Features.Users.Endpoints;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Users.Profiles;

public sealed class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, GetUserById.Response>().ReverseMap();

        CreateMap<UserRole, SearchUsers.UserRoleResponse>()
            .ConstructUsing(src => new SearchUsers.UserRoleResponse(
                src.RoleId,
                src.Role != null ? src.Role.Name : string.Empty
            ));

        CreateMap<User, SearchUsers.Response>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles ?? new List<UserRole>()))
            .ReverseMap()
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore());
        CreateMap<Paginate<User>, PaginatedListResponse<SearchUsers.Response>>().ReverseMap();
    }
}
