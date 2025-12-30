using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common;
using LifeOS.Domain.Events.RoleEvents;
using LifeOS.Persistence.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeOS.Application.Features.Roles.CreateRole;

/// <summary>
/// Rol oluşturulduğunda tetiklenen domain event handler
/// </summary>
public sealed class CreateRoleEventHandler : INotificationHandler<DomainEventNotification<RoleCreatedEvent>>
{
    private readonly ILogger<CreateRoleEventHandler> _logger;
    private readonly ICacheService _cacheService;

    public CreateRoleEventHandler(
        ILogger<CreateRoleEventHandler> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(DomainEventNotification<RoleCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        _logger.LogInformation(
            "Handling RoleCreatedEvent for Role {RoleId} - {RoleName}",
            domainEvent.RoleId,
            domainEvent.RoleName);

        try
        {
            await _cacheService.Remove(CacheKeys.RoleListVersion());

            _logger.LogInformation(
                "Cache invalidated after role {RoleId} creation",
                domainEvent.RoleId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error invalidating cache for RoleCreatedEvent {RoleId}",
                domainEvent.RoleId);
        }
    }
}

