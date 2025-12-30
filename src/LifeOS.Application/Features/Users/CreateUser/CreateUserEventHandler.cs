using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common;
using LifeOS.Domain.Events.UserEvents;
using LifeOS.Persistence.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeOS.Application.Features.Users.CreateUser;

/// <summary>
/// Kullanıcı oluşturulduğunda tetiklenen domain event handler
/// </summary>
public sealed class CreateUserEventHandler : INotificationHandler<DomainEventNotification<UserCreatedEvent>>
{
    private readonly ILogger<CreateUserEventHandler> _logger;
    private readonly ICacheService _cacheService;

    public CreateUserEventHandler(
        ILogger<CreateUserEventHandler> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(DomainEventNotification<UserCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        _logger.LogInformation(
            "Handling UserCreatedEvent for User {UserId} - {Email}",
            domainEvent.UserId,
            domainEvent.Email);

        try
        {
            await _cacheService.Remove(CacheKeys.UserListVersion());

            _logger.LogInformation(
                "Cache invalidated after user {UserId} creation",
                domainEvent.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error invalidating cache for UserCreatedEvent {UserId}",
                domainEvent.UserId);
        }
    }
}

