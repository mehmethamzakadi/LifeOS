
using AutoMapper;
using LifeOS.Application.Features.Categories.Commands.Create;
using LifeOS.Application.Features.Categories.Commands.Delete;
using LifeOS.Application.Features.Categories.Commands.Update;
using LifeOS.Application.Features.Categories.Queries.GetAll;
using LifeOS.Application.Features.Categories.Queries.GetById;
using LifeOS.Application.Features.Categories.Queries.GetPaginatedListByDynamic;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Categories.Profiles
{
    public sealed class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CreateCategoryCommand>().ReverseMap();
            CreateMap<Category, UpdateCategoryCommand>().ReverseMap();
            CreateMap<Category, DeleteCategoryCommand>().ReverseMap();

            CreateMap<Category, GetPaginatedListByDynamicCategoriesResponse>()
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : null))
                .ReverseMap();
            CreateMap<Category, GetAllListCategoriesResponse>().ReverseMap();

            CreateMap<Category, GetByIdCategoryResponse>().ReverseMap();
            CreateMap<Paginate<Category>, PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse>>().ReverseMap();


        }
    }
}
