using LifeOS.Domain.Common;
using LifeOS.Domain.Common.Utilities;
using LifeOS.Domain.Entities;
using LifeOS.Domain.Events.IntegrationEvents;
using LifeOS.Domain.Repositories;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace LifeOS.Infrastructure.Consumers;

/// <summary>
/// Consumes ActivityLogCreatedIntegrationEvent from RabbitMQ
/// and persists the activity log to the database
/// </summary>
public class ActivityLogConsumer : IConsumer<ActivityLogCreatedIntegrationEvent>
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ActivityLogConsumer> _logger;

    public ActivityLogConsumer(
        IActivityLogRepository activityLogRepository,
        IUnitOfWork unitOfWork,
        ILogger<ActivityLogConsumer> logger)
    {
        _activityLogRepository = activityLogRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ActivityLogCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        // Basit idempotency kontrolü - aynı ID'ye sahip kayıt varsa atla
        var activityLogId = context.MessageId ?? GuidHelper.GenerateDeterministicGuid(
            $"{message.EntityId}_{message.Timestamp:O}_{message.ActivityType}");

        // Mükerrer kayıt kontrolü
        var exists = await _activityLogRepository.ExistsByIdAsync(
            activityLogId,
            context.CancellationToken);

        if (exists)
        {
            _logger.LogInformation(
                "ActivityLog already exists. Skipping duplicate. Id: {ActivityLogId}",
                activityLogId);
            return;
        }

        // ✅ OpenTelemetry Trace ID'yi loglara ekle
        var activity = Activity.Current;
        var traceId = activity?.TraceId.ToString() ?? "unknown";
        var spanId = activity?.SpanId.ToString() ?? "unknown";

        _logger.LogInformation(
            "Processing ActivityLog: {ActivityType} for {EntityType} (ID: {EntityId}) [TraceId: {TraceId}, SpanId: {SpanId}]",
            message.ActivityType,
            message.EntityType,
            message.EntityId,
            traceId,
            spanId);

        var activityLog = new ActivityLog
        {
            Id = activityLogId,
            ActivityType = message.ActivityType,
            EntityType = message.EntityType,
            EntityId = message.EntityId,
            Title = message.Title,
            Details = message.Details,
            UserId = message.UserId ?? Guid.Empty,
            Timestamp = message.Timestamp
        };

        await _activityLogRepository.AddAsync(activityLog, context.CancellationToken);
        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation(
            "Successfully processed ActivityLog: {ActivityType} for {EntityType} (ID: {ActivityLogId})",
            message.ActivityType,
            message.EntityType,
            activityLogId);
    }

}
