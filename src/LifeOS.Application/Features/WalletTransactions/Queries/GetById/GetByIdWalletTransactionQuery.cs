using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.WalletTransactions.Queries.GetById;

public sealed record GetByIdWalletTransactionQuery(Guid Id) : IRequest<IDataResult<GetByIdWalletTransactionResponse>>;

