namespace Kaida.Discord.Gateway
{
    public class KaidaGatewayClient
    {
        public event Action? OnReady;
        public event Action? OnDisconnected;

        private readonly WebSocketManager _socketManager;
        private readonly HeartbeatService _heartbeatService;
        public KaidaGatewayClient() 
        {
            _socketManager = new WebSocketManager();
            _heartbeatService = new HeartbeatService();
        }

        public async Task ConnectAsync(string token)
        {
            // TODO: Open WebSocket
            // TODO: Receive HELLO
            // TODO: Start heartbeat
            // TODO: Send IDENTIFY
            // TODO: Trigger OnReady
        }

        public async Task DisconnectAsync()
        {
            // TODO: Cleanly close WebSocket & stop heartbeat
            OnDisconnected?.Invoke();
        }

    }
}
