using LifeOS.Domain.Common;
using LifeOS.Domain.Events.MusicEvents;

namespace LifeOS.Domain.Entities;

/// <summary>
/// Müzik platformu (Spotify) bağlantı bilgileri
/// </summary>
public sealed class MusicConnection : AggregateRoot
{
    // EF Core için parameterless constructor
    public MusicConnection() { }

    public Guid UserId { get; private set; }
    public string AccessToken { get; private set; } = default!; // Encrypted
    public string RefreshToken { get; private set; } = default!; // Encrypted
    public DateTime ExpiresAt { get; private set; }
    public string SpotifyUserId { get; private set; } = default!;
    public string? SpotifyUserName { get; private set; }
    public string? SpotifyUserEmail { get; private set; }
    public DateTime ConnectedAt { get; private set; }
    public DateTime? LastSyncedAt { get; private set; }
    public bool IsActive { get; private set; }

    public static MusicConnection Create(
        Guid userId,
        string accessToken,
        string refreshToken,
        DateTime expiresAt,
        string spotifyUserId,
        string? spotifyUserName = null,
        string? spotifyUserEmail = null)
    {
        var connection = new MusicConnection
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            SpotifyUserId = spotifyUserId,
            SpotifyUserName = spotifyUserName,
            SpotifyUserEmail = spotifyUserEmail,
            ConnectedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        connection.AddDomainEvent(new MusicConnectionCreatedEvent(connection.Id, userId, spotifyUserId));
        return connection;
    }

    public void UpdateTokens(string accessToken, string refreshToken, DateTime expiresAt)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
        UpdatedDate = DateTime.UtcNow;
    }

    public void UpdateSyncTime()
    {
        LastSyncedAt = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedDate = DateTime.UtcNow;
        AddDomainEvent(new MusicConnectionDeactivatedEvent(Id, UserId));
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedDate = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedDate = DateTime.UtcNow;
        IsActive = false;
        AddDomainEvent(new MusicConnectionDeletedEvent(Id, UserId));
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedDate = null;
        IsActive = true;
        UpdatedDate = DateTime.UtcNow;
    }
}

