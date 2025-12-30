using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common;
using LifeOS.Domain.Events.UserEvents;
using LifeOS.Persistence.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeOS.Application.Features.Users.AssignRolesToUser;

/// <summary>
/// Kullanıcıya roller atandığında tetiklenen domain event handler
/// </summary>
public sealed class AssignRolesToUserEventHandler : INotificationHandler<DomainEventNotification<UserRolesAssignedEvent>>
{
    private readonly ILogger<AssignRolesToUserEventHandler> _logger;
    private readonly ICacheService _cacheService;

    public AssignRolesToUserEventHandler(
        ILogger<AssignRolesToUserEventHandler> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(DomainEventNotification<UserRolesAssignedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        _logger.LogInformation(
            "Handling UserRolesAssignedEvent for User {UserId} ({UserName}) - {RoleCount} roles assigned: {RoleNames}",
            domainEvent.UserId,
            domainEvent.UserName,
            domainEvent.RoleNames.Count,
            string.Join(", ", domainEvent.RoleNames));

        try
        {
            await _cacheService.Remove(CacheKeys.UserRoles(domainEvent.UserId));
            await _cacheService.Remove(CacheKeys.UserPermissions(domainEvent.UserId));
            await _cacheService.Remove(CacheKeys.User(domainEvent.UserId));

            _logger.LogInformation(
                "Cache invalidated for user {UserId} after role assignment",
                domainEvent.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error invalidating cache for UserRolesAssignedEvent {UserId}",
                domainEvent.UserId);
        }
    }
}

