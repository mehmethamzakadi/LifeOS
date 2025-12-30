using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.Books.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicBooksQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicBooksResponse>>;

