using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QwenBotQ.NET.Models.OneBot;
using QwenBotQ.NET.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QwenBotQ.NET.Services
{
    public class OneBotService : IOneBotService
    {
        private readonly string _serverUrl;
        private readonly string? _token;
        private readonly IWebSocketService _webSocketService;
        private readonly ILogger<OneBotService> _logger;
        private readonly Dictionary<long, TaskCompletionSource<ApiResponseModel>> _apiWaiting;
        private readonly List<Func<MessageEventModel, Task>> _messageHandlers;
        private readonly List<Func<GroupMessageEventModel, Task>> _groupMessageHandlers;
        private long _selfId;

        public OneBotService(string serverUrl, string? token, IWebSocketService webSocketService, ILoggerFactory loggerFactory)
        {
            // 配置JSON序列化设置
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                NullValueHandling = NullValueHandling.Ignore,
                TypeNameHandling = TypeNameHandling.Auto,
            };

            _serverUrl = serverUrl;
            _token = token;
            _logger = loggerFactory.CreateLogger<OneBotService>();
            _webSocketService = webSocketService;
            _webSocketService.MessageReceived += HandleMessageAsync;
            _apiWaiting = new Dictionary<long, TaskCompletionSource<ApiResponseModel>>();
            _messageHandlers = new List<Func<MessageEventModel, Task>>();
            _groupMessageHandlers = new List<Func<GroupMessageEventModel, Task>>();
        }

        public async Task ConnectAsync()
        {
            _logger.LogInformation("Connecting to OneBot server...");
            await _webSocketService.ConnectAsync(_serverUrl, _token);
            _logger.LogInformation("Connected to OneBot server.");
        }

        public async Task CloseAsync()
        {
            _logger.LogInformation("Closing OneBot connection...");
            await _webSocketService.DisconnectAsync();
            _logger.LogInformation("OneBot connection closed.");
        }

        public async Task<ApiResponseModel> SendMessageAsync(List<object> message, long? userId, long? groupId)
        {
            if (userId == null && groupId == null)
            {
                throw new ArgumentException("必须指定userId或groupId其中之一");
            }

            var sendParams = new SendMessageParamsModel
            {
                UserId = userId,
                GroupId = groupId,
                Message = message
            };

            try
            {
                return await CallApiAsync<SendMessageParamsModel, ApiResponseModel>("send_msg", sendParams);
            }
            catch (TimeoutException ex)
            {
                _logger.LogError($"发送消息超时: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"发送消息失败: {ex.Message}");
                throw;
            }
        }

        public async Task<ApiResponseModel> SendTextMessageAsync(string text, long? userId, long? groupId)
        {
            var message = new List<object>
            {
                new MessageModel<TextMessageDataModel>
                {
                    Type = "text",
                    Data = new TextMessageDataModel { Text = text }
                }
            };

            try
            {
                return await SendMessageAsync(message, userId, groupId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"发送文本消息失败: {ex.Message}");
                throw;
            }
        }

        public async Task<ApiResponseModel<GetStrangerInfoDataModel>> GetStrangerInfoAsync(long userId)
        {
            var getParams = new GetStrangerInfoParamsModel
            {
                UserId = userId
            };

            return await CallApiAsync<GetStrangerInfoParamsModel, ApiResponseModel<GetStrangerInfoDataModel>>("get_stranger_info", getParams);
        }

        public async Task<ApiResponseModel<GetGroupMemberInfoDataModel>> GetGroupMemberInfoAsync(long groupId, long userId)
        {
            var getParams = new GetGroupMemberInfoParamsModel
            {
                GroupId = groupId,
                UserId = userId
            };

            return await CallApiAsync<GetGroupMemberInfoParamsModel, ApiResponseModel<GetGroupMemberInfoDataModel>>("get_group_member_info", getParams);
        }

        public async Task<MultiApiResponseModel<GetGroupMemberInfoDataModel>> GetGroupMemberListAsync(long groupId)
        {
            var getParams = new GetGroupMemberListParamsModel
            {
                GroupId = groupId
            };

            return await CallApiAsync<GetGroupMemberListParamsModel, MultiApiResponseModel<GetGroupMemberInfoDataModel>>("get_group_member_list", getParams);
        }

        public void AddMessageHandler(Func<MessageEventModel, Task> callback)
        {
            _messageHandlers.Add(callback);
        }

        public void AddGroupMessageHandler(Func<GroupMessageEventModel, Task> callback)
        {
            _groupMessageHandlers.Add(callback);
        }

        private async Task HandleMessageAsync(string data)
        {
            _logger.LogDebug($"Received data: {data}");

            try
            {
                // 尝试解析为API响应
                if (TryParseApiResponse(data, out var apiResponse, out var echo))
                {
                    // 使用Task.Run处理API响应，避免阻塞WebSocket接收循环
                    _ = Task.Run(() => ProcessApiResponse(data, apiResponse, echo));
                    return;
                }

                // 尝试解析为事件，使用Task.Run处理事件，避免阻塞WebSocket接收循环
                var eventModel = JsonConvert.DeserializeObject<BaseEventModel>(data);
                if (eventModel != null)
                {
                    _selfId = eventModel.SelfId;
                    _ = Task.Run(async () => await ProcessEventAsync(eventModel, data));
                }
                else
                {
                    _logger.LogWarning($"Received data could not be parsed as API response or event: {data}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing message: {ex.Message}");
            }
        }
        
        // 处理API响应的方法
        private void ProcessApiResponse(string data, ApiResponseModel apiResponse, long echo)
        {
            try
            {
                bool isSendMessageResponse = data.Contains("\"action\":\"send_msg\"") || 
                                           data.Contains("\"action\":\"send_private_msg\"") || 
                                           data.Contains("\"action\":\"send_group_msg\"");
                
                if (_apiWaiting.TryGetValue(echo, out var tcs))
                {
                    // 只有在任务未完成时才设置结果，避免对已超时的任务设置结果
                    if (!tcs.Task.IsCompleted)
                    {
                        tcs.SetResult(apiResponse);
                        _apiWaiting.Remove(echo);
                        
                        if (isSendMessageResponse)
                        {
                            _logger.LogInformation($"Successfully processed send_msg response with echo {echo} (message_id: {GetMessageIdFromResponse(data)})");
                        }
                        else
                        {
                            _logger.LogDebug($"Successfully processed API response with echo {echo}");
                        }
                    }
                    else
                    {
                        // 任务已完成（可能是超时），但仍然记录响应信息
                        if (isSendMessageResponse)
                        {
                            _logger.LogInformation($"Received delayed send_msg response with echo {echo} (message_id: {GetMessageIdFromResponse(data)}), but the task was already completed.");
                        }
                        else
                        {
                            _logger.LogWarning($"Received API response with echo {echo}, but the task was already completed (likely timed out).");
                        }
                        _apiWaiting.Remove(echo); // 清理已完成的任务
                    }
                }
                else
                {
                    // 对于send_msg等不需要返回值的API，记录响应但不报警
                    if (apiResponse.Status == "ok" && isSendMessageResponse)
                    {
                        _logger.LogInformation($"Received successful send_msg response with echo {echo} (message_id: {GetMessageIdFromResponse(data)}), no waiting task found (likely already processed with default response).");
                    }
                    else
                    {
                        _logger.LogWarning($"Received API response with echo {echo}, but no waiting task was found.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing API response: {ex.Message}");
            }
        }
        
        // 从send_msg响应中提取message_id
        private string GetMessageIdFromResponse(string data)
        {
            try
            {
                // 尝试从JSON中提取message_id
                var jObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
                if (jObject != null && jObject.ContainsKey("data"))
                {
                    var dataObj = jObject["data"];
                    if (dataObj != null)
                    {
                        // 尝试直接解析为字典
                        try
                        {
                            var dataDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(dataObj.ToString());
                            if (dataDict != null)
                            {
                                // 检查message_id字段（OneBot标准字段名）
                                if (dataDict.ContainsKey("message_id"))
                                {
                                    return dataDict["message_id"].ToString();
                                }
                                
                                // 检查可能的替代字段名
                                if (dataDict.ContainsKey("msg_id"))
                                {
                                    return dataDict["msg_id"].ToString();
                                }
                                
                                if (dataDict.ContainsKey("id"))
                                {
                                    return dataDict["id"].ToString();
                                }
                            }
                        }
                        catch
                        {
                            // 如果解析为字典失败，可能是直接的数值
                            if (int.TryParse(dataObj.ToString(), out int messageId))
                            {
                                return messageId.ToString();
                            }
                        }
                    }
                }
                
                // 如果data字段解析失败，尝试直接从响应中获取message_id
                if (jObject.ContainsKey("message_id"))
                {
                    return jObject["message_id"].ToString();
                }
                
                return "unknown";
            }
            catch (Exception ex)
            {
                return $"parse_error: {ex.Message}";
            }
        }

        private bool TryParseApiResponse(string data, out ApiResponseModel apiResponse, out long echo)
        {
            apiResponse = null;
            echo = 0;

            try
            {
                var jObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
                if (jObject != null && jObject.ContainsKey("echo") && jObject.ContainsKey("status"))
                {
                    apiResponse = JsonConvert.DeserializeObject<ApiResponseModel>(data);
                    echo = apiResponse.Echo;
                    _logger.LogDebug($"Successfully parsed API response with echo {echo}, status {apiResponse.Status}, retcode {apiResponse.Retcode}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                // 解析失败，不是API响应
                _logger.LogDebug($"Failed to parse as API response: {ex.Message}");
            }

            return false;
        }

        private async Task ProcessEventAsync(BaseEventModel eventModel, string rawData)
        {
            _logger.LogInformation(eventModel.ToString());

            if (eventModel.PostType == "message")
            {
                // 先检查是否为群消息
                var jObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(rawData);
                bool isGroupMessage = jObject != null && jObject.ContainsKey("message_type") && jObject["message_type"].ToString() == "group";
                
                if (isGroupMessage)
                {
                    // 如果是群消息，直接解析为 GroupMessageEventModel
                    var groupMessageEvent = JsonConvert.DeserializeObject<GroupMessageEventModel>(rawData);
                    if (groupMessageEvent != null)
                    {
                        // 注入群消息回复方法
                        InjectGroupReplyMethod(groupMessageEvent);
                        
                        // 处理消息事件（因为 GroupMessageEventModel 继承自 MessageEventModel）
                        foreach (var handler in _messageHandlers)
                        {
                            try
                            {
                                await handler(groupMessageEvent);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error in message handler: {ex.Message}");
                            }
                        }
                        
                        // 处理群消息事件
                        foreach (var handler in _groupMessageHandlers)
                        {
                            try
                            {
                                await handler(groupMessageEvent);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error in group message handler: {ex.Message}");
                            }
                        }
                    }
                }
                else
                {
                    // 如果不是群消息，解析为普通 MessageEventModel
                    var messageEvent = JsonConvert.DeserializeObject<MessageEventModel>(rawData);
                    if (messageEvent != null)
                    {
                        // 注入私信回复方法
                        InjectReplyMethod(messageEvent);

                        // 处理消息事件
                        foreach (var handler in _messageHandlers)
                        {
                            try
                            {
                                await handler(messageEvent);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError($"Error in message handler: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        private void InjectReplyMethod(MessageEventModel messageEvent)
        {
            // 如果是 GroupMessageEventModel 实例，不应该在这里注入
            if (messageEvent is GroupMessageEventModel)
            {
                return;
            }
            
            // 创建委托
            Func<List<object>, bool, Task> replyAsyncDelegate = async (message, reply) => {
                if (reply)
                {
                    message.Insert(0, new MessageModel<ReplyMessageDataModel>
                    {
                        Type = "reply",
                        Data = new ReplyMessageDataModel { Id = messageEvent.MessageId.ToString() }
                    });
                }
                await SendMessageAsync(message, messageEvent.UserId, null);
            };

            Func<string, bool, Task> replyAsyncStringDelegate = async (text, reply) => {
                await replyAsyncDelegate(new List<object>
                {
                    new MessageModel<TextMessageDataModel>
                    {
                        Type = "text",
                        Data = new TextMessageDataModel { Text = text }
                    }
                }, reply);
            };
            
            // 使用反射设置委托字段
            var replyAsyncDelegateField = typeof(MessageEventModel).GetField("_replyAsyncDelegate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var replyAsyncStringDelegateField = typeof(MessageEventModel).GetField("_replyAsyncStringDelegate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (replyAsyncDelegateField != null && replyAsyncStringDelegateField != null)
            {
                replyAsyncDelegateField.SetValue(messageEvent, replyAsyncDelegate);
                replyAsyncStringDelegateField.SetValue(messageEvent, replyAsyncStringDelegate);
            }
            else
            {
                _logger.LogError("无法找到MessageEventModel中的委托字段");
            }
        }



        private void InjectGroupReplyMethod(GroupMessageEventModel groupMessageEvent)
        {
            // 创建委托
            Func<List<object>, bool, Task> replyAsyncDelegate = async (message, reply) => {
                if (reply)
                {
                    message.Insert(0, new MessageModel<ReplyMessageDataModel>
                    {
                        Type = "reply",
                        Data = new ReplyMessageDataModel { Id = groupMessageEvent.MessageId.ToString() }
                    });
                }
                message.Insert(0, new MessageModel<AtMessageDataModel>
                {
                    Type = "at",
                    Data = new AtMessageDataModel
                    {
                        Id = groupMessageEvent.UserId.ToString()
                    }
                });
                await SendMessageAsync(message, null, groupMessageEvent.GroupId);
            };
            
            // 使用反射设置委托字段
            var replyAsyncDelegateField = typeof(GroupMessageEventModel).GetField("_replyAsyncDelegate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (replyAsyncDelegateField != null)
            {
                replyAsyncDelegateField.SetValue(groupMessageEvent, replyAsyncDelegate);
            }
            else
            {
                _logger.LogError("无法找到GroupMessageEventModel中的委托字段");
            }
        }

        private async Task<T> CallApiAsync<P, T>(string action, P args)
            where P : BaseParamsModel
            where T : ApiResponseModel
        {
            var echo = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var tcs = new TaskCompletionSource<ApiResponseModel>();
            _apiWaiting[echo] = tcs;

            var apiCall = new ApiModel<P>
            {
                Action = action,
                Params = args,
                Echo = echo
            };

            var json = JsonConvert.SerializeObject(apiCall);
            _logger.LogDebug($"Sending API call: {json}");
            await _webSocketService.SendAsync(json);

            // 对于send_msg等不需要返回值的API，使用更短的超时或直接返回空结果
            bool isFireAndForget = IsSendMessageApi(action);
            int timeoutMs = isFireAndForget ? 3000 : 10000; // send_msg使用3秒超时，其他API使用10秒
            
            // 添加超时机制，防止永久等待
            var timeoutTask = Task.Delay(timeoutMs);
            var completedTask = await Task.WhenAny(tcs.Task, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                _apiWaiting.Remove(echo);
                
                if (isFireAndForget)
                {
                    // 对于send_msg等API，超时后仍然返回一个成功的响应，避免阻塞后续操作
                    _logger.LogWarning($"Send message API call did not receive response within {timeoutMs}ms: {action}. Continuing without waiting.");
                    
                    // 创建一个默认的成功响应
                    var defaultResponse = CreateDefaultApiResponse<T>(echo);
                    return defaultResponse;
                }
                else
                {
                    // 对于其他API，仍然抛出超时异常
                    _logger.LogError($"API call timed out: {action}");
                    throw new TimeoutException($"API call timed out: {action}");
                }
            }

            var response = await tcs.Task;
            return (T)response;
        }
        
        // 判断是否为发送消息相关的API
        private bool IsSendMessageApi(string action)
        {
            return action == "send_msg" || action == "send_private_msg" || action == "send_group_msg";
        }
        
        // 创建默认的API响应
        private T CreateDefaultApiResponse<T>(long echo) where T : ApiResponseModel
        {
            // 创建一个默认的API响应对象
            var responseType = typeof(T);
            var response = (T)Activator.CreateInstance(responseType)!; // 添加null检查
            
            // 设置基本属性
            response.Echo = echo;
            response.Status = "ok";
            response.Retcode = 0;
            
            // 如果是ApiResponseModel<T>类型，尝试设置Data属性
            if (responseType.IsGenericType)
            {
                var dataType = responseType.GetGenericArguments()[0];
                var dataProperty = responseType.GetProperty("Data");
                
                if (dataProperty != null)
                {
                    // 创建一个默认的Data对象
                    var dataObj = Activator.CreateInstance(dataType);
                    
                    // 如果是发送消息的响应，设置message_id为-1
                    var messageIdProperty = dataType.GetProperty("MessageId");
                    if (messageIdProperty != null && messageIdProperty.PropertyType == typeof(int))
                    {
                        messageIdProperty.SetValue(dataObj, -1);
                    }
                    
                    if (dataObj != null) // 添加null检查
                    {
                        dataProperty.SetValue(response, dataObj);
                    }
                }
            }
            
            return response;
        }
    }
}