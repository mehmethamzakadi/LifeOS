using AutoMapper;
using LifeOS.Application.Features.Books.Commands.Create;
using LifeOS.Application.Features.Books.Commands.Delete;
using LifeOS.Application.Features.Books.Commands.Update;
using LifeOS.Application.Features.Books.Queries.GetById;
using LifeOS.Application.Features.Books.Queries.GetPaginatedListByDynamic;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Books.Profiles;

public sealed class BookProfile : Profile
{
    public BookProfile()
    {
        CreateMap<Book, CreateBookCommand>().ReverseMap();
        CreateMap<Book, UpdateBookCommand>().ReverseMap();
        CreateMap<Book, DeleteBookCommand>().ReverseMap();

        CreateMap<Book, GetPaginatedListByDynamicBooksResponse>().ReverseMap();
        CreateMap<Book, GetByIdBookResponse>().ReverseMap();
        CreateMap<Paginate<Book>, PaginatedListResponse<GetPaginatedListByDynamicBooksResponse>>().ReverseMap();
    }
}

