using Microsoft.AspNetCore.SignalR.Client;
using Thiccdal.Shared;

namespace Thiccdal.OverlayService
{
    public class OverlayConnectionService : IService
    {
        private readonly string _hubUrl;
        private readonly HubConnection _hubConnection;

        public OverlayConnectionService()
        {
            _hubUrl = "https://localhost:7154/overlayhub";
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .Build();

            _hubConnection.On<string,string>("Broadcast", BroadcastMessageHandler);
        }

        private async Task BroadcastMessageHandler(string username, string message)
        {
            Console.WriteLine($"Received message from {username}: {message}");
        }

        public Task Start(CancellationToken cancellationToken)
        {
            // NOOP
            return Task.CompletedTask;
        }

        public async Task Stop()
        {
            await _hubConnection.StopAsync();
        }

        private async Task Connect()
        {
            
        }
    }
}
