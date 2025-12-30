using AutoMapper;
using LifeOS.Application.Features.Books.Endpoints;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Books.Profiles;

public sealed class BookProfile : Profile
{
    public BookProfile()
    {
        // SearchBooks endpoint için mapping
        CreateMap<Book, SearchBooks.Response>();
        CreateMap<Paginate<Book>, PaginatedListResponse<SearchBooks.Response>>();
    }
}

