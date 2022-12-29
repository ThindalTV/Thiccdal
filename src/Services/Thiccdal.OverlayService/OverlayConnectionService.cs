using Microsoft.AspNetCore.SignalR.Client;
using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;

namespace Thiccdal.OverlayService
{
    public class OverlayConnectionService : IService
    {
        private readonly string _hubUrl;
        private readonly HubConnection _hubConnection;
        private readonly IEventAggregator _eventAggregator;

        public OverlayConnectionService(IEventAggregator eventAggregator)
        {
            _hubUrl = "https://localhost:7154/overlayhub";
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .Build();
            _eventAggregator = eventAggregator;
        }

        private async Task SubscribeToSignalRHub()
        {
            await _hubConnection.StartAsync();
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                _hubConnection.On<string, string>("Broadcast", BroadcastMessageHandler);
            }
        }

        private async Task BroadcastMessageHandler(string username, string message)
        {
            Console.WriteLine($"Received message from {username}: {message}");
            await _eventAggregator.Publish(new TestNotification1($"From OverlayConnection, recieved message from {username}, reads: {message}"));
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            while (_hubConnection.State != HubConnectionState.Connected)
            {
                await SubscribeToSignalRHub();
                await Task.Delay(1000);
            }
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
