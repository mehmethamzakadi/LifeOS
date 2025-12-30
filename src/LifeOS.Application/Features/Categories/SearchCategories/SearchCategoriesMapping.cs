using AutoMapper;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Categories.SearchCategories;

/// <summary>
/// AutoMapper profile for SearchCategories feature.
/// This mapping is specific to SearchCategories slice and kept within the slice.
/// </summary>
public sealed class SearchCategoriesMapping : Profile
{
    public SearchCategoriesMapping()
    {
        CreateMap<Category, SearchCategoriesResponse>()
            .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : null))
            .ReverseMap();
        CreateMap<Paginate<Category>, PaginatedListResponse<SearchCategoriesResponse>>().ReverseMap();
    }
}

