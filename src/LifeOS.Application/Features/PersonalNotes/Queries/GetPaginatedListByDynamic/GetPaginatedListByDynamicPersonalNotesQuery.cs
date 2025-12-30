using LifeOS.Application.Common.Requests;
using LifeOS.Application.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.PersonalNotes.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicPersonalNotesQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicPersonalNotesResponse>>;

