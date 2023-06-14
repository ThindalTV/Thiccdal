using Microsoft.AspNetCore.SignalR;
using Thiccdal.Shared.Notifications;
using Thiccdal.Shared.Notifications.Chat;

namespace Thiccdal.Frontend.SignalR
{
    public class OverlayHub : Hub
    {
        public const string HubUrl = "/overlayhub";

        public async Task Broadcast(string username, string message)
        {
            await Clients.All.SendAsync("Broadcast", username, message);
        }

        public async Task RawData(RawData rawData)
        {
            await Clients.Others.SendAsync(nameof(RawData), rawData);
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

        public override async Task OnDisconnectedAsync(Exception e)
        {
            Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(e);
        }
    }
}
