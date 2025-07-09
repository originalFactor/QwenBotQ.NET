using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QwenBotQ.SDK.Events;
using QwenBotQ.SDK.Models;
using System.Net.WebSockets;
using System.Text;

namespace QwenBotQ.SDK.Services;

public class OneBotService : IOneBotService
{
    private readonly string _serverUrl;
    private readonly string? _token;
    private readonly ILogger<OneBotService> _logger;
    private readonly Dictionary<long, TaskCompletionSource<ApiResponse>> _apiWaiting;
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly SemaphoreSlim _eventProcessingSemaphore = new(Environment.ProcessorCount * 2, Environment.ProcessorCount * 2);
    
    public event Func<MessageContext, Task>? OnMessage;
    public event Func<GroupMessageContext, Task>? OnGroupMessage;
    
    public OneBotService(string serverUrl, string? token, ILogger<OneBotService> logger)
    {
        _serverUrl = serverUrl;
        _token = token;
        _logger = logger;
        _apiWaiting = new Dictionary<long, TaskCompletionSource<ApiResponse>>();
        
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
    
    public async Task<ApiResponse> SendCQMessageAsync(string text, long? userId = null, long? groupId = null)
    {
        var message = new
        {
            user_id = userId,
            group_id = groupId,
            message = text
        };
        
        return await CallApiAsync("send_msg_rate_limited", message);
    }
    
    public async Task<ApiResponse> SendMessageAsync(List<object> message, long? userId = null, long? groupId = null)
    {
        if (userId == null && groupId == null)
        {
            throw new ArgumentException("必须指定userId或groupId其中之一");
        }
        
        var sendParams = new
        {
            user_id = userId,
            group_id = groupId,
            message
        };
        
        return await CallApiAsync("send_msg_rate_limited", sendParams);
    }
    
    public async Task<ApiResponse> GetStrangerInfoAsync(long userId)
    {
        return await CallApiAsync("get_stranger_info", new { user_id = userId });
    }
    
    public async Task<ApiResponse> GetGroupMemberInfoAsync(long groupId, long userId)
    {
        return await CallApiAsync("get_group_member_info", new { group_id = groupId, user_id = userId });
    }
    
    public async Task<ApiResponse> GetGroupMemberListAsync(long groupId)
    {
        return await CallApiAsync("get_group_member_list", new { group_id = groupId });
    }
    
    private async Task<ApiResponse> CallApiAsync(string action, object parameters)
    {
        var echo = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var tcs = new TaskCompletionSource<ApiResponse>();
        _apiWaiting[echo] = tcs;
        
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
    
    private async Task SendAsync(string message)
    {
        if (_webSocket?.State == WebSocketState.Open)
        {
            try
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
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
        try
        {
            // 尝试解析为API响应
            if (TryParseApiResponse(data, out var apiResponse))
            {
                ProcessApiResponse(apiResponse);
                return;
            }
            
            // 尝试解析为事件
            var eventData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            if (eventData != null && eventData.ContainsKey("post_type") && eventData["post_type"].ToString() == "message")
            {
                // 异步处理事件，不阻塞WebSocket消息接收，使用信号量限制并发数量
                _ = Task.Run(async () =>
                {
                    await _eventProcessingSemaphore.WaitAsync();
                    try
                    {
                        await ProcessMessageEvent(data, eventData);
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
    
    private bool TryParseApiResponse(string data, out ApiResponse apiResponse)
    {
        try
        {
            var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            if (response != null && response.ContainsKey("echo"))
            {
                apiResponse = new ApiResponse
                {
                    Status = response.GetValueOrDefault("status")?.ToString() ?? "failed",
                    Retcode = Convert.ToInt32(response.GetValueOrDefault("retcode") ?? 0),
                    Message = response.GetValueOrDefault("msg")?.ToString(),
                    Data = response.GetValueOrDefault("data"),
                    Echo = Convert.ToInt64(response["echo"])
                };
                
                return true;
            }
        }
        catch
        {
            // 解析失败
        }
        
        apiResponse = new ApiResponse { Status = "failed" };
        return false;
    }
    
    private void ProcessApiResponse(ApiResponse apiResponse)
    {
        var echo = apiResponse.Echo;
        
        if (_apiWaiting.TryGetValue(echo, out var tcs))
        {
            if (!tcs.Task.IsCompleted)
            {
                tcs.SetResult(apiResponse);
            }
            _apiWaiting.Remove(echo);
        }
    }
    
    private async Task ProcessMessageEvent(string rawData, Dictionary<string, object> eventData)
    {
        var messageType = eventData.GetValueOrDefault("message_type")?.ToString();
        
        if (messageType == "group")
        {
            var context = ParseGroupMessageContext(rawData, eventData);
            if (context != null && OnGroupMessage != null)
            {
                await OnGroupMessage(context);
            }
        }
        else if (messageType == "private")
        {
            var context = ParseMessageContext(rawData, eventData);
            if (context != null && OnMessage != null)
            {
                await OnMessage(context);
            }
        }
        else
        {
            _logger.LogWarning($"Unknown message type: {messageType}");
        }
    }
    
    private MessageContext? ParseMessageContext(string rawData, Dictionary<string, object> eventData)
    {
        try
        {
            var context = new MessageContext
            {
                SelfId = Convert.ToInt64(eventData.GetValueOrDefault("self_id") ?? 0),
                MessageId = Convert.ToInt64(eventData.GetValueOrDefault("message_id") ?? 0),
                UserId = Convert.ToInt64(eventData.GetValueOrDefault("user_id") ?? 0),
                RawMessage = eventData.GetValueOrDefault("raw_message")?.ToString() ?? "",
                Time = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(eventData.GetValueOrDefault("time") ?? 0)).DateTime
            };
            
            // 解析消息段
            if (eventData.ContainsKey("message"))
            {
                context.Message = ParseMessageSegments(eventData["message"]);
            }
            
            // 注入回复方法
            InjectReplyMethods(context);
            
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing message context");
            return null;
        }
    }
    
    private GroupMessageContext? ParseGroupMessageContext(string rawData, Dictionary<string, object> eventData)
    {
        try
        {
            var context = new GroupMessageContext
            {
                SelfId = Convert.ToInt64(eventData.GetValueOrDefault("self_id") ?? 0),
                MessageId = Convert.ToInt64(eventData.GetValueOrDefault("message_id") ?? 0),
                UserId = Convert.ToInt64(eventData.GetValueOrDefault("user_id") ?? 0),
                GroupId = Convert.ToInt64(eventData.GetValueOrDefault("group_id") ?? 0),
                RawMessage = eventData.GetValueOrDefault("raw_message")?.ToString() ?? "",
                Time = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(eventData.GetValueOrDefault("time") ?? 0)).DateTime
            };
            
            // 解析消息段
            if (eventData.ContainsKey("message"))
            {
                context.Message = ParseMessageSegments(eventData["message"]);
            }
            
            // 注入回复方法
            InjectGroupReplyMethods(context);
            
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing group message context");
            return null;
        }
    }
    
    private List<MessageSegment> ParseMessageSegments(object messageData)
    {
        var segments = new List<MessageSegment>();
        
        try
        {
            if (messageData is Newtonsoft.Json.Linq.JArray jArray)
            {
                foreach (var item in jArray)
                {
                    if (item is Newtonsoft.Json.Linq.JObject jObj)
                    {
                        var segment = new MessageSegment
                        {
                            Type = jObj["type"]?.ToString() ?? "",
                            Data = jObj["data"]?.ToObject<Dictionary<string, object>>()
                        };
                        segments.Add(segment);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing message segments");
        }
        
        return segments;
    }
    
    private void InjectReplyMethods(MessageContext context)
    {
        context.ReplyAsync = async (text, reply) =>
        {
            var message = new List<object> { new { type = "text", data = new { text } } };
            
            if (reply)
            {
                message.Insert(0, new { type = "reply", data = new { id = context.MessageId.ToString() } });
            }
            
            await SendMessageAsync(message, context.UserId, null);
        };
        
        context.ReplyMessageAsync = async (message, reply) =>
        {
            if (reply)
            {
                message.Insert(0, new { type = "reply", data = new { id = context.MessageId.ToString() } });
            }
            
            await SendMessageAsync(message, context.UserId, null);
        };
    }
    
    private void InjectGroupReplyMethods(GroupMessageContext context)
    {
        context.ReplyAsync = async (text, reply) =>
        {
            var message = new List<object>
            {
                new { type = "at", data = new { qq = context.UserId.ToString() } },
                new { type = "text", data = new { text } }
            };
            
            if (reply)
            {
                message.Insert(0, new { type = "reply", data = new { id = context.MessageId.ToString() } });
            }
            
            await SendMessageAsync(message, null, context.GroupId);
        };
        
        context.ReplyMessageAsync = async (message, reply) =>
        {
            message.Insert(0, new { type = "at", data = new { qq = context.UserId.ToString() } });
            
            if (reply)
            {
                message.Insert(0, new { type = "reply", data = new { id = context.MessageId.ToString() } });
            }
            
            await SendMessageAsync(message, null, context.GroupId);
        };
    }
}