using AutoMapper;
using LifeOS.Application.Features.PersonalNotes.Commands.Create;
using LifeOS.Application.Features.PersonalNotes.Commands.Delete;
using LifeOS.Application.Features.PersonalNotes.Commands.Update;
using LifeOS.Application.Features.PersonalNotes.Queries.GetById;
using LifeOS.Application.Features.PersonalNotes.Queries.GetPaginatedListByDynamic;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.PersonalNotes.Profiles;

public sealed class PersonalNoteProfile : Profile
{
    public PersonalNoteProfile()
    {
        CreateMap<PersonalNote, CreatePersonalNoteCommand>().ReverseMap();
        CreateMap<PersonalNote, UpdatePersonalNoteCommand>().ReverseMap();
        CreateMap<PersonalNote, DeletePersonalNoteCommand>().ReverseMap();

        CreateMap<PersonalNote, GetPaginatedListByDynamicPersonalNotesResponse>().ReverseMap();
        CreateMap<PersonalNote, GetByIdPersonalNoteResponse>().ReverseMap();
        CreateMap<Paginate<PersonalNote>, PaginatedListResponse<GetPaginatedListByDynamicPersonalNotesResponse>>().ReverseMap();
    }
}

