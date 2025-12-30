using AutoMapper;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.WalletTransactions.SearchWalletTransactions;

/// <summary>
/// AutoMapper profile for SearchWalletTransactions feature.
/// This mapping is specific to SearchWalletTransactions slice and kept within the slice.
/// </summary>
public sealed class SearchWalletTransactionsMapping : Profile
{
    public SearchWalletTransactionsMapping()
    {
        CreateMap<WalletTransaction, SearchWalletTransactionsResponse>().ReverseMap();
        CreateMap<Paginate<WalletTransaction>, PaginatedListResponse<SearchWalletTransactionsResponse>>().ReverseMap();
    }
}

