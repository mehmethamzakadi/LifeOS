using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.PersonalNotes.Queries.GetById;

public sealed record GetByIdPersonalNoteQuery(Guid Id) : IRequest<IDataResult<GetByIdPersonalNoteResponse>>;

