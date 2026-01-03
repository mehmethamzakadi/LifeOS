using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LifeOS.API.Hubs;

/// <summary>
/// Müzik ile ilgili real-time güncellemeler için SignalR hub
/// </summary>
[Authorize]
public class MusicHub : Hub
{
    /// <summary>
    /// Kullanıcı bağlandığında kendi userId'sine göre group'a ekle
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            // Her kullanıcı kendi userId'sine göre bir group'a eklenir
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Kullanıcı bağlantısı kesildiğinde group'tan çıkar
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}

