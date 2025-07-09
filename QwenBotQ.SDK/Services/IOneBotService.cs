using QwenBotQ.SDK.Events;
using QwenBotQ.SDK.Models;

namespace QwenBotQ.SDK.Services;

public interface IOneBotService
{
    /// <summary>
    /// 启动OneBot服务
    /// </summary>
    Task StartAsync();
    
    /// <summary>
    /// 停止OneBot服务
    /// </summary>
    Task StopAsync();
    
    /// <summary>
    /// 发送文本消息
    /// </summary>
    Task<ApiResponse> SendCQMessageAsync(string text, long? userId = null, long? groupId = null);
    
    /// <summary>
    /// 发送消息
    /// </summary>
    Task<ApiResponse> SendMessageAsync(List<object> message, long? userId = null, long? groupId = null);
    
    /// <summary>
    /// 获取陌生人信息
    /// </summary>
    Task<ApiResponse> GetStrangerInfoAsync(long userId);
    
    /// <summary>
    /// 获取群成员信息
    /// </summary>
    Task<ApiResponse> GetGroupMemberInfoAsync(long groupId, long userId);
    
    /// <summary>
    /// 获取群成员列表
    /// </summary>
    Task<ApiResponse> GetGroupMemberListAsync(long groupId);

    /// <summary>
    /// 消息事件
    /// </summary>
    event Func<MessageContext, Task> OnMessage;
    
    /// <summary>
    /// 群消息事件
    /// </summary>
    event Func<GroupMessageContext, Task> OnGroupMessage;
}