using LifeOS.Application.Common;

namespace LifeOS.Application.Features.Roles.Queries.GetList;

public sealed record GetListRoleResponse : BaseEntityResponse
{
    public string Name { get; init; } = string.Empty;
}
