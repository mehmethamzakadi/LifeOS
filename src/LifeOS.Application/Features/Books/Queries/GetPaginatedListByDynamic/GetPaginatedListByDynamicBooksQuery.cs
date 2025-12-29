using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.Books.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicBooksQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicBooksResponse>>;

