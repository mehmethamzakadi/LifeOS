using LifeOS.Application.Abstractions;
using LifeOS.Domain.Common;

namespace LifeOS.Infrastructure.Services;

public sealed class CurrentUserService(IExecutionContextAccessor executionContextAccessor) : ICurrentUserService
{
    public Guid? GetCurrentUserId()
    {
        return executionContextAccessor.GetCurrentUserId();
    }
}
