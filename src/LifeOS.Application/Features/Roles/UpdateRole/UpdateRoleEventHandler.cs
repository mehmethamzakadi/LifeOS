using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common;
using LifeOS.Domain.Events.RoleEvents;
using LifeOS.Persistence.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeOS.Application.Features.Roles.UpdateRole;

/// <summary>
/// Rol güncellendiğinde tetiklenen domain event handler
/// </summary>
public sealed class UpdateRoleEventHandler : INotificationHandler<DomainEventNotification<RoleUpdatedEvent>>
{
    private readonly ILogger<UpdateRoleEventHandler> _logger;
    private readonly ICacheService _cacheService;

    public UpdateRoleEventHandler(
        ILogger<UpdateRoleEventHandler> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(DomainEventNotification<RoleUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        _logger.LogInformation(
            "Handling RoleUpdatedEvent for Role {RoleId} - {RoleName}",
            domainEvent.RoleId,
            domainEvent.RoleName);

        try
        {
            await _cacheService.Remove(CacheKeys.Role(domainEvent.RoleId));
            await _cacheService.Remove(CacheKeys.RolePermissions(domainEvent.RoleId));
            await _cacheService.Remove(CacheKeys.RoleListVersion());

            _logger.LogInformation(
                "Cache invalidated for role {RoleId} after update",
                domainEvent.RoleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error invalidating cache for RoleUpdatedEvent {RoleId}",
                domainEvent.RoleId);
        }
    }
}

