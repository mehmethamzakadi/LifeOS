using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.Categories.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicCategoriesQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicCategoriesResponse>>;
