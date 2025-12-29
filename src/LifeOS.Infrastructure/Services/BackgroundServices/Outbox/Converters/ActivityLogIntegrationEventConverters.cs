using LifeOS.Application.Abstractions;
using LifeOS.Domain.Constants;
using LifeOS.Domain.Events.BookEvents;
using LifeOS.Domain.Events.CategoryEvents;
using LifeOS.Domain.Events.GameEvents;
using LifeOS.Domain.Events.IntegrationEvents;
using LifeOS.Domain.Events.MovieSeriesEvents;
using LifeOS.Domain.Events.PermissionEvents;
using LifeOS.Domain.Events.PersonalNoteEvents;
using LifeOS.Domain.Events.RoleEvents;
using LifeOS.Domain.Events.UserEvents;
using LifeOS.Domain.Events.WalletTransactionEvents;
using System.Text.Json;

namespace LifeOS.Infrastructure.Services.BackgroundServices.Outbox.Converters;

public interface IIntegrationEventConverterStrategy
{
    string EventType { get; }
    object? Convert(string payload);
}

internal abstract class ActivityLogIntegrationEventConverter<TDomainEvent> : IIntegrationEventConverterStrategy
{
    public abstract string EventType { get; }

    public object? Convert(string payload)
    {
        var domainEvent = JsonSerializer.Deserialize<TDomainEvent>(payload);
        return domainEvent is null
            ? null
            : Convert(domainEvent);
    }

    protected abstract ActivityLogCreatedIntegrationEvent Convert(TDomainEvent domainEvent);
}

internal sealed class CategoryCreatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<CategoryCreatedEvent>
{
    public override string EventType => nameof(CategoryCreatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(CategoryCreatedEvent domainEvent) => new(
        ActivityType: "category_created",
        EntityType: "Category",
        EntityId: domainEvent.CategoryId,
        Title: $"\"{domainEvent.Name}\" kategorisi oluşturuldu",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class CategoryUpdatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<CategoryUpdatedEvent>
{
    public override string EventType => nameof(CategoryUpdatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(CategoryUpdatedEvent domainEvent) => new(
        ActivityType: "category_updated",
        EntityType: "Category",
        EntityId: domainEvent.CategoryId,
        Title: $"\"{domainEvent.Name}\" kategorisi güncellendi",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class CategoryDeletedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<CategoryDeletedEvent>
{
    public override string EventType => nameof(CategoryDeletedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(CategoryDeletedEvent domainEvent) => new(
        ActivityType: "category_deleted",
        EntityType: "Category",
        EntityId: domainEvent.CategoryId,
        Title: $"\"{domainEvent.Name}\" kategorisi silindi",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class UserCreatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<UserCreatedEvent>
{
    public override string EventType => nameof(UserCreatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(UserCreatedEvent domainEvent) => new(
        ActivityType: "user_created",
        EntityType: "User",
        EntityId: domainEvent.UserId,
        Title: $"Kullanıcı \"{domainEvent.UserName}\" oluşturuldu",
        Details: $"Email: {domainEvent.Email}",
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class UserUpdatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<UserUpdatedEvent>
{
    public override string EventType => nameof(UserUpdatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(UserUpdatedEvent domainEvent) => new(
        ActivityType: "user_updated",
        EntityType: "User",
        EntityId: domainEvent.UserId,
        Title: $"Kullanıcı \"{domainEvent.UserName}\" güncellendi",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class UserDeletedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<UserDeletedEvent>
{
    public override string EventType => nameof(UserDeletedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(UserDeletedEvent domainEvent) => new(
        ActivityType: "user_deleted",
        EntityType: "User",
        EntityId: domainEvent.UserId,
        Title: $"Kullanıcı \"{domainEvent.UserName}\" silindi",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class UserRolesAssignedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<UserRolesAssignedEvent>
{
    public override string EventType => nameof(UserRolesAssignedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(UserRolesAssignedEvent domainEvent) => new(
        ActivityType: "user_roles_assigned",
        EntityType: "User",
        EntityId: domainEvent.UserId,
        Title: $"Kullanıcı \"{domainEvent.UserName}\" için roller atandı",
        Details: $"Roller: {string.Join(", ", domainEvent.RoleNames)}",
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class RoleCreatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<RoleCreatedEvent>
{
    public override string EventType => nameof(RoleCreatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(RoleCreatedEvent domainEvent) => new(
        ActivityType: "role_created",
        EntityType: "Role",
        EntityId: domainEvent.RoleId,
        Title: $"Rol \"{domainEvent.RoleName}\" oluşturuldu",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class RoleUpdatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<RoleUpdatedEvent>
{
    public override string EventType => nameof(RoleUpdatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(RoleUpdatedEvent domainEvent) => new(
        ActivityType: "role_updated",
        EntityType: "Role",
        EntityId: domainEvent.RoleId,
        Title: $"Rol \"{domainEvent.RoleName}\" güncellendi",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class RoleDeletedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<RoleDeletedEvent>
{
    public override string EventType => nameof(RoleDeletedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(RoleDeletedEvent domainEvent) => new(
        ActivityType: "role_deleted",
        EntityType: "Role",
        EntityId: domainEvent.RoleId,
        Title: $"Rol \"{domainEvent.RoleName}\" silindi",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class PermissionsAssignedToRoleIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<PermissionsAssignedToRoleEvent>
{
    public override string EventType => nameof(PermissionsAssignedToRoleEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(PermissionsAssignedToRoleEvent domainEvent) => new(
        ActivityType: "permissions_assigned_to_role",
        EntityType: "Role",
        EntityId: domainEvent.RoleId,
        Title: $"Rol \"{domainEvent.RoleName}\" için yetkiler atandı",
        Details: $"{domainEvent.PermissionNames.Count} yetki atandı",
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

// Book Events
internal sealed class BookCreatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<BookCreatedEvent>
{
    public override string EventType => nameof(BookCreatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(BookCreatedEvent domainEvent) => new(
        ActivityType: "book_created",
        EntityType: "Book",
        EntityId: domainEvent.BookId,
        Title: $"\"{domainEvent.Title}\" kitabı oluşturuldu",
        Details: $"Yazar: {domainEvent.Author}",
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class BookUpdatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<BookUpdatedEvent>
{
    public override string EventType => nameof(BookUpdatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(BookUpdatedEvent domainEvent) => new(
        ActivityType: "book_updated",
        EntityType: "Book",
        EntityId: domainEvent.BookId,
        Title: $"\"{domainEvent.Title}\" kitabı güncellendi",
        Details: $"Yazar: {domainEvent.Author}",
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class BookDeletedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<BookDeletedEvent>
{
    public override string EventType => nameof(BookDeletedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(BookDeletedEvent domainEvent) => new(
        ActivityType: "book_deleted",
        EntityType: "Book",
        EntityId: domainEvent.BookId,
        Title: $"\"{domainEvent.Title}\" kitabı silindi",
        Details: $"Yazar: {domainEvent.Author}",
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

// Game Events
internal sealed class GameCreatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<GameCreatedEvent>
{
    public override string EventType => nameof(GameCreatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(GameCreatedEvent domainEvent) => new(
        ActivityType: "game_created",
        EntityType: "Game",
        EntityId: domainEvent.GameId,
        Title: $"\"{domainEvent.Title}\" oyunu oluşturuldu",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class GameUpdatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<GameUpdatedEvent>
{
    public override string EventType => nameof(GameUpdatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(GameUpdatedEvent domainEvent) => new(
        ActivityType: "game_updated",
        EntityType: "Game",
        EntityId: domainEvent.GameId,
        Title: $"\"{domainEvent.Title}\" oyunu güncellendi",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class GameDeletedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<GameDeletedEvent>
{
    public override string EventType => nameof(GameDeletedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(GameDeletedEvent domainEvent) => new(
        ActivityType: "game_deleted",
        EntityType: "Game",
        EntityId: domainEvent.GameId,
        Title: $"\"{domainEvent.Title}\" oyunu silindi",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

// MovieSeries Events
internal sealed class MovieSeriesCreatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<MovieSeriesCreatedEvent>
{
    public override string EventType => nameof(MovieSeriesCreatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(MovieSeriesCreatedEvent domainEvent) => new(
        ActivityType: "movieseries_created",
        EntityType: "MovieSeries",
        EntityId: domainEvent.MovieSeriesId,
        Title: $"\"{domainEvent.Title}\" film/dizisi oluşturuldu",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class MovieSeriesUpdatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<MovieSeriesUpdatedEvent>
{
    public override string EventType => nameof(MovieSeriesUpdatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(MovieSeriesUpdatedEvent domainEvent) => new(
        ActivityType: "movieseries_updated",
        EntityType: "MovieSeries",
        EntityId: domainEvent.MovieSeriesId,
        Title: $"\"{domainEvent.Title}\" film/dizisi güncellendi",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class MovieSeriesDeletedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<MovieSeriesDeletedEvent>
{
    public override string EventType => nameof(MovieSeriesDeletedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(MovieSeriesDeletedEvent domainEvent) => new(
        ActivityType: "movieseries_deleted",
        EntityType: "MovieSeries",
        EntityId: domainEvent.MovieSeriesId,
        Title: $"\"{domainEvent.Title}\" film/dizisi silindi",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

// PersonalNote Events
internal sealed class PersonalNoteCreatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<PersonalNoteCreatedEvent>
{
    public override string EventType => nameof(PersonalNoteCreatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(PersonalNoteCreatedEvent domainEvent) => new(
        ActivityType: "personalnote_created",
        EntityType: "PersonalNote",
        EntityId: domainEvent.PersonalNoteId,
        Title: $"\"{domainEvent.Title}\" notu oluşturuldu",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class PersonalNoteUpdatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<PersonalNoteUpdatedEvent>
{
    public override string EventType => nameof(PersonalNoteUpdatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(PersonalNoteUpdatedEvent domainEvent) => new(
        ActivityType: "personalnote_updated",
        EntityType: "PersonalNote",
        EntityId: domainEvent.PersonalNoteId,
        Title: $"\"{domainEvent.Title}\" notu güncellendi",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class PersonalNoteDeletedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<PersonalNoteDeletedEvent>
{
    public override string EventType => nameof(PersonalNoteDeletedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(PersonalNoteDeletedEvent domainEvent) => new(
        ActivityType: "personalnote_deleted",
        EntityType: "PersonalNote",
        EntityId: domainEvent.PersonalNoteId,
        Title: $"\"{domainEvent.Title}\" notu silindi",
        Details: null,
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

// WalletTransaction Events
internal sealed class WalletTransactionCreatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<WalletTransactionCreatedEvent>
{
    public override string EventType => nameof(WalletTransactionCreatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(WalletTransactionCreatedEvent domainEvent) => new(
        ActivityType: "wallettransaction_created",
        EntityType: "WalletTransaction",
        EntityId: domainEvent.WalletTransactionId,
        Title: $"\"{domainEvent.Title}\" cüzdan işlemi oluşturuldu",
        Details: $"Tutar: {domainEvent.Amount:C}",
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class WalletTransactionUpdatedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<WalletTransactionUpdatedEvent>
{
    public override string EventType => nameof(WalletTransactionUpdatedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(WalletTransactionUpdatedEvent domainEvent) => new(
        ActivityType: "wallettransaction_updated",
        EntityType: "WalletTransaction",
        EntityId: domainEvent.WalletTransactionId,
        Title: $"\"{domainEvent.Title}\" cüzdan işlemi güncellendi",
        Details: $"Tutar: {domainEvent.Amount:C}",
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}

internal sealed class WalletTransactionDeletedIntegrationEventConverter(IExecutionContextAccessor executionContextAccessor) : ActivityLogIntegrationEventConverter<WalletTransactionDeletedEvent>
{
    public override string EventType => nameof(WalletTransactionDeletedEvent);

    protected override ActivityLogCreatedIntegrationEvent Convert(WalletTransactionDeletedEvent domainEvent) => new(
        ActivityType: "wallettransaction_deleted",
        EntityType: "WalletTransaction",
        EntityId: domainEvent.WalletTransactionId,
        Title: $"\"{domainEvent.Title}\" cüzdan işlemi silindi",
        Details: $"Tutar: {domainEvent.Amount:C}",
        UserId: executionContextAccessor.GetCurrentUserId() ?? SystemUsers.SystemUserId,
        Timestamp: DateTime.UtcNow
    );
}