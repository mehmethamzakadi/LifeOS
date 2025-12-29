using AutoMapper;
using LifeOS.Application.Features.WalletTransactions.Commands.Create;
using LifeOS.Application.Features.WalletTransactions.Commands.Delete;
using LifeOS.Application.Features.WalletTransactions.Commands.Update;
using LifeOS.Application.Features.WalletTransactions.Queries.GetById;
using LifeOS.Application.Features.WalletTransactions.Queries.GetPaginatedListByDynamic;
using LifeOS.Domain.Common.Paging;
using LifeOS.Domain.Common.Responses;
using LifeOS.Domain.Entities;

namespace LifeOS.Application.Features.WalletTransactions.Profiles;

public sealed class WalletTransactionProfile : Profile
{
    public WalletTransactionProfile()
    {
        CreateMap<WalletTransaction, CreateWalletTransactionCommand>().ReverseMap();
        CreateMap<WalletTransaction, UpdateWalletTransactionCommand>().ReverseMap();
        CreateMap<WalletTransaction, DeleteWalletTransactionCommand>().ReverseMap();

        CreateMap<WalletTransaction, GetPaginatedListByDynamicWalletTransactionsResponse>().ReverseMap();
        CreateMap<WalletTransaction, GetByIdWalletTransactionResponse>().ReverseMap();
        CreateMap<Paginate<WalletTransaction>, PaginatedListResponse<GetPaginatedListByDynamicWalletTransactionsResponse>>().ReverseMap();
    }
}

