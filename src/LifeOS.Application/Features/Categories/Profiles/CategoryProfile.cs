using AutoMapper;
using LifeOS.Application.Features.Categories.Endpoints;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Categories.Profiles;

public sealed class CategoryProfile : Profile
{
    public CategoryProfile()
    {
        CreateMap<Category, SearchCategories.Response>()
            .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : null))
            .ReverseMap();
        CreateMap<Paginate<Category>, PaginatedListResponse<SearchCategories.Response>>().ReverseMap();
    }
}
