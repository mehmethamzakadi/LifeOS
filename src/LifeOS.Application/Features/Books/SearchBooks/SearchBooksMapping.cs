using AutoMapper;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.Books.SearchBooks;

/// <summary>
/// AutoMapper profile for SearchBooks feature.
/// This mapping is specific to SearchBooks slice and kept within the slice.
/// </summary>
public sealed class SearchBooksMapping : Profile
{
    public SearchBooksMapping()
    {
        CreateMap<Book, SearchBooksResponse>();
        CreateMap<Paginate<Book>, PaginatedListResponse<SearchBooksResponse>>();
    }
}

