using Microsoft.Extensions.Logging;
using QwenBotQ.NET.Services.Interfaces;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QwenBotQ.NET.Services
{
    public class WebSocketService : IWebSocketService
    {
        private readonly ILogger<WebSocketService> _logger;
        private string _uri = string.Empty; // 初始化为空字符串
        private string? _token;
        private ClientWebSocket _ws;
        private CancellationTokenSource _cancellationTokenSource;

        public event Func<string, Task> MessageReceived = _ => Task.CompletedTask; // 初始化为空任务

        public WebSocketService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WebSocketService>();
            _ws = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public WebSocketService(string uri, string? token, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WebSocketService>();
            _uri = uri;
            _token = token;
            _ws = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public async Task ConnectAsync()
        {
            if (string.IsNullOrEmpty(_uri))
            {
                throw new InvalidOperationException("Server URL is not set. Please call ConnectAsync(serverUrl, token) instead.");
            }
            await ConnectAsync(_uri, _token);
        }

        public async Task ConnectAsync(string serverUrl, string? token)
        {
            _uri = serverUrl;
            _token = token;
            _logger.LogInformation($"Connecting to server at {_uri}");
            
            // 确保旧的连接已关闭
            await CleanupConnectionAsync();
            
            // 创建新的WebSocket客户端和取消令牌
            _ws = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();
            
            if (_token != null)
            {
                _logger.LogDebug($"Using token authentication");
                _ws.Options.SetRequestHeader("Authorization", $"Bearer {_token}");
            }
            
            try
            {
                // 设置连接超时
                var connectTask = _ws.ConnectAsync(new Uri(_uri), _cancellationTokenSource.Token);
                var timeoutTask = Task.Delay(10000, _cancellationTokenSource.Token); // 10秒连接超时
                
                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException("Connection attempt timed out.");
                }
                
                // 确保连接任务完成
                await connectTask;
                
                if (_ws.State == WebSocketState.Open)
                {
                    _logger.LogInformation("Connected to server successfully.");
                    // 启动接收循环
                    _ = Task.Run(ReceiveLoop, _cancellationTokenSource.Token);
                }
                else
                {
                    throw new WebSocketException($"WebSocket is in unexpected state after connection: {_ws.State}");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Connection attempt was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error connecting to server: {ex.Message}");
                // 尝试重连，但不要在初始连接时无限重试
                await ReconnectAsync();
            }
        }
        
        private async Task CleanupConnectionAsync()
        {
            // 取消所有正在进行的操作
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
            }
            
            // 关闭WebSocket连接
            if (_ws != null)
            {
                if (_ws.State == WebSocketState.Open)
                {
                    try
                    {
                        // 使用短超时，避免长时间等待
                        var closeTask = _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Cleanup before reconnect", CancellationToken.None);
                        var timeoutTask = Task.Delay(1000); // 1秒超时
                        
                        await Task.WhenAny(closeTask, timeoutTask);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Error during WebSocket cleanup: {ex.Message}");
                    }
                }
                
                try
                {
                    _ws.Dispose();
                }
                catch
                {
                    // 忽略释放时的错误
                }
            }
        }

        public async Task DisconnectAsync()
        {
            _logger.LogInformation("Disconnecting from server...");
            await CleanupConnectionAsync();
            _logger.LogInformation("Disconnected successfully.");
        }

        public async Task SendAsync(string message)
        {
            // 检查连接状态
            if (_ws == null || _ws.State != WebSocketState.Open)
            {
                _logger.LogWarning("WebSocket is not open. Attempting to reconnect before sending message.");
                try
                {
                    // 尝试重新连接
                    await ReconnectAsync();
                    
                    // 如果重连后仍未连接，则放弃发送
                    if (_ws == null || _ws.State != WebSocketState.Open)
                    {
                        _logger.LogError("Failed to reconnect. Cannot send message.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error reconnecting: {ex.Message}");
                    return;
                }
            }
            
            // 发送消息
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);
            
            try
            {
                // 使用超时机制发送消息
                var sendTask = _ws.SendAsync(segment, WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
                var timeoutTask = Task.Delay(5000); // 5秒超时
                
                var completedTask = await Task.WhenAny(sendTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    _logger.LogError("Send message timed out.");
                    await ReconnectAsync();
                    return;
                }
                
                await sendTask; // 确保任务完成
                _logger.LogDebug($"Sent message: {message}");
            }
            catch (WebSocketException wex)
            {
                _logger.LogError($"WebSocket error while sending message: {wex.Message}");
                await ReconnectAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                await ReconnectAsync();
            }
        }

        private async Task ReceiveLoop()
        {
            var buffer = new byte[1024 * 16]; // 增大缓冲区大小
            
            while (_ws != null && _ws.State == WebSocketState.Open && !_cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    MemoryStream? messageBuffer = null;
                    WebSocketReceiveResult? result = null;
                    
                    try
                    {
                        messageBuffer = new MemoryStream();
                        
                        do
                        {
                            // 检查连接状态
                            if (_ws.State != WebSocketState.Open || _cancellationTokenSource.IsCancellationRequested)
                            {
                                _logger.LogWarning("WebSocket connection closed during message reception.");
                                break;
                            }
                            
                            result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                            
                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                _logger.LogWarning("WebSocket connection closed by peer.");
                                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                                await ReconnectAsync();
                                return;
                            }
                            
                            await messageBuffer.WriteAsync(buffer, 0, result.Count, _cancellationTokenSource.Token);
                        } 
                        while (result != null && !result.EndOfMessage);
                        
                        // 如果连接已关闭或取消，则不处理消息
                        if (_ws.State != WebSocketState.Open || _cancellationTokenSource.IsCancellationRequested)
                        {
                            continue;
                        }
                        
                        // 重置流位置到开始
                        messageBuffer.Position = 0;
                        
                        // 将接收到的数据转换为字符串
                        string message;
                        using (var reader = new StreamReader(messageBuffer, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
                        {
                            message = await reader.ReadToEndAsync();
                        }
                        
                        _logger.LogDebug($"Received message: {message}");

                        // 触发消息接收事件，使用Task.Run避免阻塞接收循环
                        if (MessageReceived != null && !string.IsNullOrEmpty(message))
                        {
                            // 复制消息内容，避免在异步处理过程中被修改
                            string messageCopy = message;
                            
                            // 使用Task.Run在后台线程处理消息，避免阻塞接收循环
                            _ = Task.Run(async () => 
                            {
                                try 
                                {
                                    await MessageReceived.Invoke(messageCopy);
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError($"Error in message handler: {ex.Message}");
                                }
                            });
                        }
                    }
                    finally
                    {
                        // 确保释放资源
                        if (messageBuffer != null)
                        {
                            await messageBuffer.DisposeAsync();
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("WebSocket receive operation was canceled.");
                    break;
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
            
            _logger.LogInformation("WebSocket receive loop ended.");
        }

        private async Task ReconnectAsync()
        {
            // 如果已经请求取消，则不进行重连
            if (_cancellationTokenSource.IsCancellationRequested) 
            {
                _logger.LogInformation("Reconnection canceled: cancellation requested.");
                return;
            }

            // 使用指数退避策略，避免频繁重连
            int retryCount = 0;
            int maxRetries = 5;
            bool connected = false;
            
            // 创建新的取消令牌源，确保旧的操作被取消
            try
            {
                if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error canceling previous operations: {ex.Message}");
            }
            
            _cancellationTokenSource = new CancellationTokenSource();

            while (!connected && retryCount < maxRetries && !_cancellationTokenSource.IsCancellationRequested)
            {
                int delaySeconds = (int)Math.Pow(2, retryCount); // 指数退避：1, 2, 4, 8, 16秒
                _logger.LogInformation($"Attempting to reconnect in {delaySeconds} seconds... (Attempt {retryCount + 1}/{maxRetries})");
                
                try
                {
                    await Task.Delay(delaySeconds * 1000, _cancellationTokenSource.Token);
                    
                    // 清理旧连接
                    await CleanupConnectionAsync();
                    
                    // 创建新的WebSocket客户端
                    _ws = new ClientWebSocket();
                    
                    if (_token != null)
                    {
                        _ws.Options.SetRequestHeader("Authorization", $"Bearer {_token}");
                    }
                    
                    // 设置连接超时
                    var connectTask = _ws.ConnectAsync(new Uri(_uri), _cancellationTokenSource.Token);
                    var timeoutTask = Task.Delay(10000, _cancellationTokenSource.Token); // 10秒连接超时
                    
                    var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                    if (completedTask == timeoutTask)
                    {
                        throw new TimeoutException("Connection attempt timed out.");
                    }
                    
                    // 确保连接任务完成
                    await connectTask;
                    
                    if (_ws.State == WebSocketState.Open)
                    {
                        _logger.LogInformation("Reconnected to server successfully.");
                        
                        // 重新启动接收循环
                        _ = Task.Run(ReceiveLoop, _cancellationTokenSource.Token);
                        
                        connected = true;
                    }
                    else
                    {
                        throw new WebSocketException($"WebSocket is in unexpected state: {_ws.State}");
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Reconnection attempt was canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Reconnection attempt {retryCount + 1} failed: {ex.Message}");
                    retryCount++;
                }
            }

            if (!connected && !_cancellationTokenSource.IsCancellationRequested)
            {
                _logger.LogError($"Failed to reconnect after {maxRetries} attempts. Please restart the application.");
            }
        }
    }
}