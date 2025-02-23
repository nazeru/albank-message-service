using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MessageService.Dal;

namespace MessageService.Api
{
    public class WebSocketHandler
    {
        private readonly ConcurrentDictionary<Guid, WebSocket> _clients = new();
        private readonly ILogger<WebSocketHandler> _logger;

        public WebSocketHandler(ILogger<WebSocketHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleConnection(WebSocket webSocket)
        {
            var clientId = Guid.NewGuid();
            _clients.TryAdd(clientId, webSocket);
            _logger.LogInformation($"{clientId} - Client connected");

            try
            {
                var buffer = new byte[1024 * 4];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning($"{clientId} - Client disconnected.");
                        _clients.TryRemove(clientId, out _);
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", CancellationToken.None);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{clientId} - Error WebSocket: {ex.Message}");
                _clients.TryRemove(clientId, out _);
            }
        }

        public async Task SendMessageToClients(Message message)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(jsonMessage);

            foreach (var client in _clients.Values)
            {
                if (client.State == WebSocketState.Open)
                {
                    _logger.LogInformation($"Send message to client: {jsonMessage}");
                    await client.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
