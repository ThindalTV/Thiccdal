using Microsoft.AspNetCore.SignalR;
using Thiccdal.Shared.Notifications.Chat;
using Thiccdal.Shared.Notifications;

namespace Thiccdal.WebSocketHost;

class CommunicationsHub : Hub
{
    public const string HubUrl = "/hub";

    public async Task Broadcast(string username, string message)
    {
        await Clients.All.SendAsync("Broadcast", username, message);
    }

    public async Task RawData(RawData rawData)
    {
        await Clients.Others.SendAsync(nameof(RawData), rawData);
    }

    public async Task OutgoingChatMessage(ChatMessage message)
    {
        await Clients.Others.SendAsync(nameof(OutgoingChatMessage), message);
    }

    public async Task IncomingChatMessage(ChatMessage message)
    {
        await Clients.Others.SendAsync(nameof(IncomingChatMessage), message);
    }

    public async Task ChatMessage(ChatMessage rawData)
    {
        await Clients.Others.SendAsync(nameof(ChatMessage), rawData);
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"{Context.ConnectionId} connected");
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? e)
    {
        Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
        await base.OnDisconnectedAsync(e);
    }
}