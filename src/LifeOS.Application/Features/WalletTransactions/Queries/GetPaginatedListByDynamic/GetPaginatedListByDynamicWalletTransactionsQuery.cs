using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.WalletTransactions.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicWalletTransactionsQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicWalletTransactionsResponse>>;

