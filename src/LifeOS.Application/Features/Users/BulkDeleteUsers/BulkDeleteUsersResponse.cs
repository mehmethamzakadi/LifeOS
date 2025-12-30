namespace LifeOS.Application.Features.Users.BulkDeleteUsers;

public sealed record BulkDeleteUsersResponse(
    int DeletedCount,
    int FailedCount,
    List<string> Errors);

