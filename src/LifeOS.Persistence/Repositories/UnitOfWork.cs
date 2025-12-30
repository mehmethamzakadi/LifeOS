using LifeOS.Domain.Common;
using LifeOS.Persistence.Common;
using LifeOS.Persistence.Contexts;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace LifeOS.Persistence.Repositories;

/// <summary>
/// LifeOSDbContext için Unit of Work implementasyonu
/// Domain event orchestration ve transaction yönetimi
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly LifeOSDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public UnitOfWork(
        LifeOSDbContext context,
        IMediator mediator,
        ILogger<UnitOfWork> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = GetDomainEvents().ToList();

        try
        {
            if (!domainEvents.Any())
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }

            var existingTransaction = _context.Database.CurrentTransaction;
            if (existingTransaction != null)
            {
                return await SaveWithinTransaction(domainEvents, cancellationToken);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await SaveWithinTransaction(domainEvents, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        finally
        {
            // Her durumda domain event'leri temizle (başarılı veya hatalı durumda)
            ClearDomainEvents();
        }
    }

    private async Task<int> SaveWithinTransaction(List<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        // 1. Önce entity değişikliklerini kaydet
        var result = await _context.SaveChangesAsync(cancellationToken);

        // 2. Domain event'leri MediatR ile yayınla (in-process handlers için)
        // Bu sayede cache invalidation, logging gibi side-effect'ler hemen gerçekleşir
        // NOT: IDomainEvent artık MediatR'dan bağımsız, DomainEventNotification wrapper kullanılıyor
        foreach (var domainEvent in domainEvents)
        {
            try
            {
                _logger.LogDebug(
                    "Publishing domain event {EventType} for aggregate {AggregateId}",
                    domainEvent.GetType().Name,
                    domainEvent.AggregateId);

                // Domain event'i MediatR notification'a dönüştür
                var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
                var notification = Activator.CreateInstance(notificationType, domainEvent);
                
                if (notification != null)
                {
                    await _mediator.Publish(notification, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                // Domain event handler hatası transaction'ı bozmasın
                // Ama logla ki monitoring'de görülebilsin
                _logger.LogError(ex,
                    "Error publishing domain event {EventType} for aggregate {AggregateId}",
                    domainEvent.GetType().Name,
                    domainEvent.AggregateId);
            }
        }

        return result;
    }

    public int SaveChanges()
    {
        return _context.SaveChanges();
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public IEnumerable<IDomainEvent> GetDomainEvents()
    {
        // Tüm track edilen entity'lerden domain event'leri topla
        // EntityState'e bakmadan (Unchanged, Modified, Added, Deleted hepsi dahil)
        // Çünkü entity'de property değişikliği olmasa bile domain event eklenebilir
        return _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .SelectMany(e => e.Entity.DomainEvents)
            .ToList();
    }

    public void ClearDomainEvents()
    {
        var entities = _context.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Sadece kendi transaction'ımızı dispose et
            // DbContext DI container tarafından yönetiliyor, dispose edilmemeli
            _transaction?.Dispose();
        }
        _disposed = true;
    }
}