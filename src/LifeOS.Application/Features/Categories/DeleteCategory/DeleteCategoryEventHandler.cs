using LifeOS.Application.Abstractions;
using LifeOS.Application.Common.Caching;
using LifeOS.Domain.Common;
using LifeOS.Domain.Events.CategoryEvents;
using LifeOS.Persistence.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LifeOS.Application.Features.Categories.DeleteCategory;

/// <summary>
/// Kategori silindiğinde tetiklenen domain event handler
/// </summary>
public sealed class DeleteCategoryEventHandler : INotificationHandler<DomainEventNotification<CategoryDeletedEvent>>
{
    private readonly ILogger<DeleteCategoryEventHandler> _logger;
    private readonly ICacheService _cacheService;

    public DeleteCategoryEventHandler(
        ILogger<DeleteCategoryEventHandler> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task Handle(DomainEventNotification<CategoryDeletedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        
        _logger.LogInformation(
            "Handling CategoryDeletedEvent for Category {CategoryId} - {Name}",
            domainEvent.CategoryId,
            domainEvent.Name);

        try
        {
            // Invalidate specific category caches
            await _cacheService.Remove(CacheKeys.Category(domainEvent.CategoryId));
            
            // Invalidate category list version to invalidate all cached category lists
            await _cacheService.Remove(CacheKeys.CategoryListVersion());
            
            // Also invalidate category grid version
            await _cacheService.Remove(CacheKeys.CategoryGridVersion());

            _logger.LogInformation(
                "Cache invalidated for deleted category {CategoryId}",
                domainEvent.CategoryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error invalidating cache for CategoryDeletedEvent {CategoryId}",
                domainEvent.CategoryId);
        }
    }
}

