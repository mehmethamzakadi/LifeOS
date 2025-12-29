using AutoMapper;
using LifeOS.Application.Features.Roles.Queries.GetList;
using LifeOS.Application.Features.Roles.Queries.GetRoleById;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Roles.Profiles
{
    public class RoleProfile : Profile
    {
        public RoleProfile()
        {
            CreateMap<Role, GetListRoleResponse>().ReverseMap();
            CreateMap<Role, GetRoleByIdQueryResponse>().ReverseMap();

            CreateMap<Paginate<Role>, PaginatedListResponse<GetListRoleResponse>>().ReverseMap();
        }
    }
}
