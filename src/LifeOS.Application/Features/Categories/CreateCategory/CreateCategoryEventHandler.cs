using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common;
using LifeOS.Domain.Events.CategoryEvents;
using LifeOS.Persistence.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeOS.Application.Features.Categories.CreateCategory;

/// <summary>
/// Kategori oluşturulduğunda tetiklenen domain event handler
/// Cache invalidation ve side-effect'leri yönetir
/// </summary>
public sealed class CreateCategoryEventHandler : INotificationHandler<DomainEventNotification<CategoryCreatedEvent>>
{
    private readonly ILogger<CreateCategoryEventHandler> _logger;
    private readonly ICacheService _cacheService;

    public CreateCategoryEventHandler(
        ILogger<CreateCategoryEventHandler> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(DomainEventNotification<CategoryCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        _logger.LogInformation(
            "Handling CategoryCreatedEvent for Category {CategoryId} - {Name}",
            domainEvent.CategoryId,
            domainEvent.Name);

        try
        {
            // Invalidate category list version to invalidate all cached category lists
            await _cacheService.Remove(CacheKeys.CategoryListVersion());
            
            // Also invalidate category grid version
            await _cacheService.Remove(CacheKeys.CategoryGridVersion());

            _logger.LogInformation(
                "Cache invalidated after category {CategoryId} creation",
                domainEvent.CategoryId);
        }
        catch (Exception ex)
        {
            // Cache hatası kritik değil, log ve devam et
            _logger.LogError(ex,
                "Error invalidating cache for CategoryCreatedEvent {CategoryId}",
                domainEvent.CategoryId);
        }
    }
}

