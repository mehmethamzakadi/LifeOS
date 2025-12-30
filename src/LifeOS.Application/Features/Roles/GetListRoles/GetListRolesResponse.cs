using LifeOS.Application.Common;

namespace LifeOS.Application.Features.Roles.GetListRoles;

public sealed record GetListRolesResponse : BaseEntityResponse
{
    public string Name { get; init; } = string.Empty;
}

