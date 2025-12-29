using LifeOS.Domain.Common.Requests;
using LifeOS.Domain.Common.Responses;
using MediatR;

namespace LifeOS.Application.Features.PersonalNotes.Queries.GetPaginatedListByDynamic;

public sealed record GetPaginatedListByDynamicPersonalNotesQuery(DataGridRequest DataGridRequest) : IRequest<PaginatedListResponse<GetPaginatedListByDynamicPersonalNotesResponse>>;

