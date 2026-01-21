using System.Net.WebSockets;

namespace Kaida.Discord.Gateway
{
    internal class WebSocketManager
    {
        private ClientWebSocket _socket = new ClientWebSocket();

        public async Task ConnectAsync(string gatewayUrl, CancellationToken token)
        {
            // TODO: Connect to Discord Gateway
        }

        public async Task SendAsync(string payload)
        {
            // TODO: Send JSON payload over WebSocket
        }

        public async Task<string> ReceiveAsync()
        {
            // TODO: Receive raw JSON payload from WebSocket
            return "";
        }

        public async Task CloseAsync()
        {
            // TODO: Close WebSocket
        }
    }
}