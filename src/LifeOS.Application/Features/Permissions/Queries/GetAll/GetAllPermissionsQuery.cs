using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Permissions.Queries.GetAll;

public record GetAllPermissionsQuery : IRequest<IDataResult<GetAllPermissionsResponse>>;
