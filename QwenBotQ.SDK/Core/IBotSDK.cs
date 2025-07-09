using QwenBotQ.SDK.Commands;
using QwenBotQ.SDK.Events;
using QwenBotQ.SDK.Services;

namespace QwenBotQ.SDK.Core;

public interface IBotSDK
{
    /// <summary>
    /// 启动Bot SDK
    /// </summary>
    Task StartAsync();
    
    /// <summary>
    /// 停止Bot SDK
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// 消息事件
    /// </summary>
    event Func<MessageContext, Task> OnMessage;
    
    /// <summary>
    /// 群消息事件
    /// </summary>
    event Func<GroupMessageContext, Task> OnGroupMessage;

    CommandManager CommandManagerService { get; }
    IOneBotService OneBotService { get; }
    IDatabaseService DataBaseService { get; }
}