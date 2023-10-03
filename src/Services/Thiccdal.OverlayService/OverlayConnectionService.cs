using Microsoft.AspNetCore.SignalR.Client;
using Thiccdal.Shared;
using Thiccdal.Shared.EventAggregator;
using Thiccdal.Shared.Notifications;
using Thiccdal.Shared.Notifications.Chat;

namespace Thiccdal.OverlayService
{
    public class OverlayConnectionService : IService, IEventSubscriber
    {
        private readonly string _hubUrl;
        private readonly HubConnection _hubConnection;
        private readonly IEventAggregator _eventAggregator;
        private bool disposedValue;

        public OverlayConnectionService(IEventAggregator eventAggregator)
        {
            _hubUrl = "http://localhost:1234/hub";
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .Build();
            _eventAggregator = eventAggregator;

            _eventAggregator.Subscribe<RawData>(this, _ => true, RawDataNotificationHandler);
            _eventAggregator.Subscribe<IncomingChatMessage>(this, _ => true, ChatMessageHandler);
        }

        private async Task RawDataNotificationHandler(RawData notification, CancellationToken cancellationToken)
        {
            await SendToHub(nameof(RawData), notification, cancellationToken);
        }
        private async Task ChatMessageHandler(ChatMessage message, CancellationToken cancellationToken)
        {
            await SendToHub(nameof(IncomingChatMessage), message, cancellationToken);
        }



        private async Task SubscribeToSignalRHub()
        {
            while(_hubConnection.State == HubConnectionState.Connecting)
            {
                // Wait for the connection to be established
                await Task.Delay(500);
            }

            if (_hubConnection.State != HubConnectionState.Connected)
            {
                await _hubConnection.StartAsync();
                _hubConnection.On<string, string>("Broadcast", BroadcastMessageHandler);
                _hubConnection.On<OutgoingChatMessage>(nameof(OutgoingChatMessage), OutgoingChatMessageHandler);
                _hubConnection.On<IncomingChatMessage>(nameof(IncomingChatMessage), IncomingChatMessageHandler);
            }
        }
        void OutgoingChatMessageHandler(OutgoingChatMessage message)
        {
            _eventAggregator.Publish(message, this);
        }

        void IncomingChatMessageHandler(IncomingChatMessage message)
        {
            _eventAggregator.Publish(message, this);
        }

        private async Task SendToHub(string endPoint, object payload, CancellationToken cancellationToken)
        {
            if (_hubConnection.State != HubConnectionState.Connected)
            {
                  await SubscribeToSignalRHub();
            }

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.SendAsync(endPoint, payload, cancellationToken);
            }
        }

        private async Task BroadcastMessageHandler(string username, string message)
        {
            Console.WriteLine($"Received message from {username}: {message}");
            await _eventAggregator.Publish(new TestNotification1($"From OverlayConnection, recieved message from {username}, reads: {message}"));
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            do
            {
                await SubscribeToSignalRHub();
                await Task.Delay(1000);
            } while (_hubConnection.State != HubConnectionState.Connected);
        }

        public async Task Stop()
        {
            _eventAggregator.Unsubscribe<RawData>(this);
            await _hubConnection.StopAsync();
        }

        private async Task Connect()
        {

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
