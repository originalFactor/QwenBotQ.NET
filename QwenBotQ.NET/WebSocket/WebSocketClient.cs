using System.Text;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;

namespace QwenBotQ.NET.WebSocket
{
    internal class WebSocketClient
    {
        ClientWebSocket _ws;
        string _uri;
        string? _token;
        CancellationTokenSource _cancellationTokenSource;
        ILogger _logger;
        public delegate Task WSHandler(string data);
        public event WSHandler ?WSEvent;

        public WebSocketClient(string? url = null, string ? token = null,  ILogger? logger = null)
        {
            _ws = new ClientWebSocket();
            _uri = url ?? "ws://127.0.0.1:3001/";
            _token = token;
            _cancellationTokenSource = new CancellationTokenSource();
            _logger = logger ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WebSocketClient>();
        }

        public async Task ConnectAsync()
        {
            _logger.LogInformation($"Connecting to server at {_uri}");
            if (_token != null)
            {
                _logger.LogInformation($"Using token {_token}");
                _ws.Options.SetRequestHeader("Authorization", $"Bearer {_token}");
            }
            try
            {
                await _ws.ConnectAsync(new Uri(_uri), _cancellationTokenSource.Token);
                _logger.LogInformation("Connected to server successfully.");
                _ = Task.Run(ReceiveLoop, _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error connecting to server: {ex.Message}");
                await ReconnectAsync();
            }
        }

        async Task ReceiveLoop()
        {
            var buffer = new byte[1024 * 8];
            while (_ws.State == WebSocketState.Open && !_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        _logger.LogError("WebSocket connection closed by peer.");
                        await ReconnectAsync();
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogDebug($"Received message: {message}");

                    WSEvent?.Invoke(message);
                }
                catch (WebSocketException wex)
                {
                    _logger.LogError($"WebSocket error: {wex.Message}");
                    await ReconnectAsync();
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error receiving message: {ex.Message}");
                    await ReconnectAsync();
                    break;
                }
            }
        }

        async Task ReconnectAsync()
        {
            if (_cancellationTokenSource.IsCancellationRequested) return;

            _logger.LogInformation("Attempting to reconnect in 5 seconds...");
            await Task.Delay(5000, _cancellationTokenSource.Token);

            try
            {
                _ws.Dispose();
                _ws = new ClientWebSocket();
                await ConnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Reconnection failed: {ex.Message}");
                await ReconnectAsync();
            }
        }

        public async Task SendAsync(string message)
        {
            if (_ws.State != WebSocketState.Open)
            {
                _logger.LogError("WebSocket is not open. Cannot send message.");
                return;
            }
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);
            try
            {
                await _ws.SendAsync(segment, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
                _logger.LogDebug($"Sent message: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                await ReconnectAsync();
            }
        }

        public async Task DisconnectAsync()
        {
            if (_ws.State == WebSocketState.Open)
            {
                _logger.LogInformation("Disconnecting from server...");
                try
                {
                    await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
                    _logger.LogInformation("Disconnected successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error during disconnection: {ex.Message}");
                }
            }
            _cancellationTokenSource.Cancel();
            _ws.Dispose();
        }
    }
}
