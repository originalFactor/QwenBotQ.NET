using QwenBotQ.NET.Models.OneBot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QwenBotQ.NET.Services.Interfaces
{
    public interface IOneBotService
    {
        /// <summary>
        /// 连接到OneBot服务器
        /// </summary>
        Task ConnectAsync();

        /// <summary>
        /// 关闭与OneBot服务器的连接
        /// </summary>
        Task CloseAsync();

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="userId">用户ID（私聊）</param>
        /// <param name="groupId">群ID（群聊）</param>
        /// <returns>发送结果</returns>
        Task<ApiResponseModel> SendMessageAsync(List<object> message, long? userId, long? groupId);

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <param name="userId">用户ID（私聊）</param>
        /// <param name="groupId">群ID（群聊）</param>
        /// <returns>发送结果</returns>
        Task<ApiResponseModel> SendTextMessageAsync(string text, long? userId, long? groupId);

        /// <summary>
        /// 获取陌生人信息
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <returns>用户信息</returns>
        Task<ApiResponseModel<GetStrangerInfoDataModel>> GetStrangerInfoAsync(long userId);

        /// <summary>
        /// 获取群成员信息
        /// </summary>
        /// <param name="groupId">群ID</param>
        /// <param name="userId">用户ID</param>
        /// <returns>群成员信息</returns>
        Task<ApiResponseModel<GetGroupMemberInfoDataModel>> GetGroupMemberInfoAsync(long groupId, long userId);

        /// <summary>
        /// 获取群成员列表
        /// </summary>
        /// <param name="groupId">群ID</param>
        /// <returns>群成员列表</returns>
        Task<MultiApiResponseModel<GetGroupMemberInfoDataModel>> GetGroupMemberListAsync(long groupId);

        /// <summary>
        /// 添加消息处理回调
        /// </summary>
        /// <param name="callback">回调函数</param>
        void AddMessageHandler(Func<MessageEventModel, Task> callback);

        /// <summary>
        /// 添加群消息处理回调
        /// </summary>
        /// <param name="callback">回调函数</param>
        void AddGroupMessageHandler(Func<GroupMessageEventModel, Task> callback);
    }
}