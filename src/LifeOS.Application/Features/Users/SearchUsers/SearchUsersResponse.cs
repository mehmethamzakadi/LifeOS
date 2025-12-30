using LifeOS.Application.Common;

namespace LifeOS.Application.Features.Users.SearchUsers;

public sealed record SearchUsersResponse : BaseEntityResponse
{
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public IReadOnlyCollection<UserRoleResponse> Roles { get; init; } = Array.Empty<UserRoleResponse>();
}

public sealed record UserRoleResponse(Guid Id, string Name);

