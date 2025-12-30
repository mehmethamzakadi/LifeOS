using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.WalletTransactions.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicWalletTransactionsQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicWalletTransactionsResponse>>;

