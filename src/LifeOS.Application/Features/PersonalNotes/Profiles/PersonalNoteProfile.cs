using AutoMapper;
using LifeOS.Application.Features.PersonalNotes.Endpoints;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.PersonalNotes.Profiles;

public sealed class PersonalNoteProfile : Profile
{
    public PersonalNoteProfile()
    {
        CreateMap<PersonalNote, SearchPersonalNotes.Response>().ReverseMap();
        CreateMap<Paginate<PersonalNote>, PaginatedListResponse<SearchPersonalNotes.Response>>().ReverseMap();
    }
}

