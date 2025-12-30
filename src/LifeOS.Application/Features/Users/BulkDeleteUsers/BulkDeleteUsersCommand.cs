namespace LifeOS.Application.Features.Users.BulkDeleteUsers;

public sealed record BulkDeleteUsersCommand(List<Guid> UserIds);

