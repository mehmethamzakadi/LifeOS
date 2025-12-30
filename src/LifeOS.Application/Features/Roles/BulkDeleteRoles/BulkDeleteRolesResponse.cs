namespace LifeOS.Application.Features.Roles.BulkDeleteRoles;

public sealed record BulkDeleteRolesResponse(
    int DeletedCount,
    int FailedCount,
    List<string> Errors);

