using AutoMapper;
using LifeOS.Application.Features.WalletTransactions.Endpoints;
using LifeOS.Domain.Common.Paging;
using LifeOS.Application.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.WalletTransactions.Profiles;

public sealed class WalletTransactionProfile : Profile
{
    public WalletTransactionProfile()
    {
        CreateMap<WalletTransaction, SearchWalletTransactions.Response>().ReverseMap();
        CreateMap<Paginate<WalletTransaction>, PaginatedListResponse<SearchWalletTransactions.Response>>().ReverseMap();
    }
}

