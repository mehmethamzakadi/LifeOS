using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common;
using LifeOS.Domain.Events.UserEvents;
using LifeOS.Persistence.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeOS.Application.Features.Users.UpdateUser;

/// <summary>
/// Kullanıcı güncellendiğinde tetiklenen domain event handler
/// </summary>
public sealed class UpdateUserEventHandler : INotificationHandler<DomainEventNotification<UserUpdatedEvent>>
{
    private readonly ILogger<UpdateUserEventHandler> _logger;
    private readonly ICacheService _cacheService;

    public UpdateUserEventHandler(
        ILogger<UpdateUserEventHandler> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(DomainEventNotification<UserUpdatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        _logger.LogInformation(
            "Handling UserUpdatedEvent for User {UserId}",
            domainEvent.UserId);

        try
        {
            await _cacheService.Remove(CacheKeys.User(domainEvent.UserId));
            await _cacheService.Remove(CacheKeys.UserRoles(domainEvent.UserId));
            await _cacheService.Remove(CacheKeys.UserPermissions(domainEvent.UserId));
            await _cacheService.Remove(CacheKeys.UserListVersion());

            _logger.LogInformation(
                "Cache invalidated for user {UserId} after update",
                domainEvent.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error invalidating cache for UserUpdatedEvent {UserId}",
                domainEvent.UserId);
        }
    }
}

