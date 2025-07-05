using System;
using System.Threading.Tasks;

namespace QwenBotQ.NET.Services.Interfaces
{
    public interface IWebSocketService
    {
        /// <summary>
        /// 连接到WebSocket服务器
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// 连接到指定的WebSocket服务器
        /// </summary>
        /// <param name="serverUrl">服务器URL</param>
        /// <param name="token">认证令牌</param>
        Task ConnectAsync(string serverUrl, string? token);

        /// <summary>
        /// 断开与WebSocket服务器的连接
        /// </summary>
        Task DisconnectAsync();

        /// <summary>
        /// 发送消息到WebSocket服务器
        /// </summary>
        /// <param name="message">要发送的消息</param>
        Task SendAsync(string message);

        /// <summary>
        /// 当收到消息时触发的事件
        /// </summary>
        event Func<string, Task> MessageReceived;
    }
}