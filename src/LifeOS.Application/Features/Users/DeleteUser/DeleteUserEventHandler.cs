using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common;
using LifeOS.Domain.Events.UserEvents;
using LifeOS.Persistence.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeOS.Application.Features.Users.DeleteUser;

/// <summary>
/// Kullanıcı silindiğinde tetiklenen domain event handler
/// </summary>
public sealed class DeleteUserEventHandler : INotificationHandler<DomainEventNotification<UserDeletedEvent>>
{
    private readonly ILogger<DeleteUserEventHandler> _logger;
    private readonly ICacheService _cacheService;

    public DeleteUserEventHandler(
        ILogger<DeleteUserEventHandler> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(DomainEventNotification<UserDeletedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        _logger.LogInformation(
            "Handling UserDeletedEvent for User {UserId}",
            domainEvent.UserId);

        try
        {
            await _cacheService.Remove(CacheKeys.User(domainEvent.UserId));
            await _cacheService.Remove(CacheKeys.UserRoles(domainEvent.UserId));
            await _cacheService.Remove(CacheKeys.UserPermissions(domainEvent.UserId));
            await _cacheService.Remove(CacheKeys.UserListVersion());

            _logger.LogInformation(
                "Cache invalidated for deleted user {UserId}",
                domainEvent.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error invalidating cache for UserDeletedEvent {UserId}",
                domainEvent.UserId);
        }
    }
}

