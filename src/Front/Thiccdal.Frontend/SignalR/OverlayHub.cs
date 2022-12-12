using Microsoft.AspNetCore.SignalR;

namespace Thiccdal.Frontend.SignalR
{
    public class OverlayHub : Hub
    {
        public const string HubUrl = "/overlayhub";

        public async Task Broadcast(string username, string message)
        {
            await Clients.AllExcept(Context.ConnectionId).SendAsync("Broadcast", username, message);
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
