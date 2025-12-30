using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common;
using LifeOS.Domain.Events.RoleEvents;
using LifeOS.Persistence.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeOS.Application.Features.Roles.DeleteRole;

/// <summary>
/// Rol silindiğinde tetiklenen domain event handler
/// </summary>
public sealed class DeleteRoleEventHandler : INotificationHandler<DomainEventNotification<RoleDeletedEvent>>
{
    private readonly ILogger<DeleteRoleEventHandler> _logger;
    private readonly ICacheService _cacheService;

    public DeleteRoleEventHandler(
        ILogger<DeleteRoleEventHandler> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(DomainEventNotification<RoleDeletedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        _logger.LogInformation(
            "Handling RoleDeletedEvent for Role {RoleId} - {RoleName}",
            domainEvent.RoleId,
            domainEvent.RoleName);

        try
        {
            await _cacheService.Remove(CacheKeys.Role(domainEvent.RoleId));
            await _cacheService.Remove(CacheKeys.RolePermissions(domainEvent.RoleId));
            await _cacheService.Remove(CacheKeys.RoleListVersion());

            _logger.LogInformation(
                "Cache invalidated for deleted role {RoleId}",
                domainEvent.RoleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error invalidating cache for RoleDeletedEvent {RoleId}",
                domainEvent.RoleId);
        }
    }
}

