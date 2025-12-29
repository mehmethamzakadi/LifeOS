using LifeOS.Application.Abstractions;

namespace LifeOS.Infrastructure.Services;

public sealed class CurrentUserService(IExecutionContextAccessor executionContextAccessor) : ICurrentUserService
{
    public Guid? GetCurrentUserId()
    {
        return executionContextAccessor.GetCurrentUserId();
    }
}
