using LifeOS.Domain.Common.Results;
using MediatR;

namespace LifeOS.Application.Features.Permissions.Queries.GetRolePermissions;

public record GetRolePermissionsQuery(Guid RoleId) : IRequest<IDataResult<GetRolePermissionsResponse>>;
