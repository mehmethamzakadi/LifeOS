using AutoMapper;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.PersonalNotes.SearchPersonalNotes;

/// <summary>
/// AutoMapper profile for SearchPersonalNotes feature.
/// This mapping is specific to SearchPersonalNotes slice and kept within the slice.
/// </summary>
public sealed class SearchPersonalNotesMapping : Profile
{
    public SearchPersonalNotesMapping()
    {
        CreateMap<PersonalNote, SearchPersonalNotesResponse>().ReverseMap();
        CreateMap<Paginate<PersonalNote>, PaginatedListResponse<SearchPersonalNotesResponse>>().ReverseMap();
    }
}

