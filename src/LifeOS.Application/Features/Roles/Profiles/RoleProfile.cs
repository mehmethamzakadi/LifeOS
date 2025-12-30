using AutoMapper;
using LifeOS.Application.Features.Roles.Endpoints;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Roles.Profiles
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<Role, GetListRoles.Response>().ReverseMap();
            CreateMap<Role, GetRoleById.Response>().ReverseMap();

            CreateMap<Paginate<Role>, PaginatedListResponse<GetListRoles.Response>>().ReverseMap();
        }
    }
}
