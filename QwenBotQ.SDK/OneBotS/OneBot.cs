using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QwenBotQ.SDK.Context;
using QwenBotQ.SDK.Messages;
using QwenBotQ.SDK.Models;
using QwenBotQ.SDK.Models.OneBot.API;
using QwenBotQ.SDK.Models.OneBot.Events;
using System.Net.WebSockets;
using System.Text;

namespace QwenBotQ.SDK.OneBotS;

public partial class OneBot
{
    private readonly string _serverUrl;
    private readonly string? _token;
    private readonly ILogger<OneBot> _logger;
    private readonly Dictionary<long, Tuple<Type, TaskCompletionSource<ApiResponse>>> _apiWaiting;
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly SemaphoreSlim _eventProcessingSemaphore = new(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);
    
    public Dictionary<Type, List<Func<BaseEvent, Task>>> EventHandlers { get; } = new();

    public OneBot(string serverUrl, string? token, ILogger<OneBot> logger)
    {
        _serverUrl = serverUrl;
        _token = token;
        _logger = logger;
        _apiWaiting = new Dictionary<long, Tuple<Type, TaskCompletionSource<ApiResponse>>>();

        // 配置JSON序列化设置
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            NullValueHandling = NullValueHandling.Ignore
        };
    }
    
    public async Task StartAsync()
    {
        _logger.LogInformation("Starting OneBot service...");
        
        _cancellationTokenSource = new CancellationTokenSource();
        _webSocket = new ClientWebSocket();
        
        if (!string.IsNullOrEmpty(_token))
        {
            _webSocket.Options.SetRequestHeader("Authorization", $"Bearer {_token}");
        }
        
        await _webSocket.ConnectAsync(new Uri(_serverUrl), _cancellationTokenSource.Token);
        
        // 启动接收循环
        _ = Task.Run(ReceiveLoop, _cancellationTokenSource.Token);
        
        _logger.LogInformation("OneBot service started successfully.");
    }
    
    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping OneBot service...");
        
        _cancellationTokenSource?.Cancel();
        
        if (_webSocket?.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Service stopping", CancellationToken.None);
        }
        
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
        _eventProcessingSemaphore?.Dispose();
        
        _logger.LogInformation("OneBot service stopped.");
    }

    private async Task<ApiResponse> CallApiAsync(string action, object? parameters = null, Type? type = null)
    {
        var echo = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var tcs = new TaskCompletionSource<ApiResponse>();
        _apiWaiting[echo] = Tuple.Create(type ?? typeof(ApiResponse), tcs);

        var apiCall = new
        {
            action,
            @params = parameters,
            echo
        };
        
        var json = JsonConvert.SerializeObject(apiCall);
        
        await SendAsync(json);

        // 30秒超时
        var timeoutTask = Task.Delay(30000);
        var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);

        if (completedTask == timeoutTask)
        {
            _apiWaiting.Remove(echo);
            throw new TimeoutException($"API call timed out: {action}");
        }
        return await tcs.Task;
    }
    
    private async Task<T> CallApiAsync<T>(string action, object? parameters = null) where T : ApiResponse
    {
        return (T)(await CallApiAsync(action, parameters, typeof(T)));
    }

    private async Task SendAsync(string message)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                _logger.LogTrace($"Websocket Send: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WebSocket message");
                throw;
            }
        }
        else
        {
            _logger.LogWarning($"WebSocket is not open. Current state: {_webSocket?.State}");
            throw new InvalidOperationException($"WebSocket is not open. Current state: {_webSocket?.State}");
        }
    }
    
    private async Task ReceiveLoop()
    {
        var buffer = new byte[8192]; // 增大缓冲区
        var messageBuilder = new StringBuilder();
        
        try
        {
            while (_webSocket?.State == WebSocketState.Open && !_cancellationTokenSource!.Token.IsCancellationRequested)
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var fragment = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    messageBuilder.Append(fragment);
                    
                    // 如果消息结束，处理完整消息
                    if (result.EndOfMessage)
                    {
                        var completeMessage = messageBuilder.ToString();
                        messageBuilder.Clear();
                        ProcessMessage(completeMessage);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常取消
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in receive loop");
        }
    }
    
    private void ProcessMessage(string data)
    {
        _logger.LogTrace($"Websocket Receive: {data}");
        try
        {
            // 尝试解析为对象
            var dataObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            if(dataObj == null)
            {
                _logger.LogWarning("Received invalid JSON data: {Data}", data);
                return;
            }

            // 尝试解析为API响应
            if (dataObj.ContainsKey("echo"))
            {
                ProcessApiResponse(data, Convert.ToInt64(dataObj["echo"]));
                return;
            }
            
            // 尝试解析为事件
            if (dataObj.ContainsKey("post_type"))
            {
                // 异步处理事件，不阻塞WebSocket消息接收，使用信号量限制并发数量
                _ = Task.Run(async () =>
                {
                    await _eventProcessingSemaphore.WaitAsync();
                    try
                    {
                        await ProcessEvent(data);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing message event asynchronously");
                    }
                    finally
                    {
                        _eventProcessingSemaphore.Release();
                    }
                });
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message: {Data}", data);
        }
    }
    
    private void ProcessApiResponse(string data, long echo)
    {        
        if (_apiWaiting.TryGetValue(echo, out var tuple))
        {
            var (type, tcs) = tuple;
            if (!tcs.Task.IsCompleted)
            {
                tcs.SetResult((ApiResponse)JsonConvert.DeserializeObject(data, type)!);
            }
            else
            {
                _logger.LogWarning($"API response for echo {echo} already completed. Ignoring duplicate response.");
            }
            _apiWaiting.Remove(echo);
        }
        else
        {
            _logger.LogWarning($"Received API response for unknown echo: {echo}\n{data}");
        }
    }
    
    private async Task ProcessEvent(string rawData)
    {
        var _event = JsonConvert.DeserializeObject<BaseEvent>(rawData);
        if(_event == null)
        {
            _logger.LogWarning("Received invalid event data: {RawData}", rawData);
            return;
        }

        var assignableHandlersPack = EventHandlers
            .Where(kvp => kvp.Key.IsAssignableFrom(_event.GetType()))
            .Select(kvp => kvp.Value);
        foreach (var handlers in assignableHandlersPack)
        {
            foreach (var h in handlers)
            {
                await h.Invoke(_event);
            }
        }
    }

    public void RegisterEventHandler<T>(Func<T, Task> handler) where T : BaseEvent
    {
        if (!EventHandlers.TryGetValue(typeof(T), out var handlers))
        {
            handlers = new List<Func<BaseEvent, Task>>();
            EventHandlers[typeof(T)] = handlers;
        }
        handlers.Add(e => handler((T)e));
    }
}