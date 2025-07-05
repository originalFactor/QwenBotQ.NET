using JsonSubTypes;
using Newtonsoft.Json;
using System.Text;

namespace QwenBotQ.NET.Models.OneBot
{
    /// <summary>
    /// 事件模型基类
    /// </summary>
    [JsonConverter(typeof(JsonSubtypes), "post_type")]
    [JsonSubtypes.KnownSubType(typeof(MessageEventModel), "message")]
    [JsonSubtypes.FallBackSubType(typeof(BaseEventModel))]
    public class BaseEventModel
    {
        public required string PostType { get; set; }
        public long Time { get; set; }
        public long SelfId { get; set; }

        public override string ToString()
        {
            return $"[{PostType}]";
        }
    }

    /// <summary>
    /// 消息事件模型
    /// </summary>
    [JsonConverter(typeof(JsonSubtypes), "message_type")]
    [JsonSubtypes.KnownSubType(typeof(GroupMessageEventModel), "group")]
    [JsonSubtypes.FallBackSubType(typeof(MessageEventModel))]
    public class MessageEventModel : BaseEventModel
    {
        // 用于存储委托的私有字段
        private Func<List<object>, bool, Task>? _replyAsyncDelegate = null;
        private Func<string, bool, Task>? _replyAsyncStringDelegate = null;

        public required string MessageType { get; init; }
        public required string SubType { get; init; }
        public long UserId { get; init; }
        public long MessageId { get; init; }
        public required List<MessageModel> Message { get; init; }
        public required string RawMessage { get; init; }
        public int Font { get; init; }
        public required SenderModel Sender { get; init; }

        /// <summary>
        /// 回复消息
        /// </summary>
        /// <param name="message">消息内容</param>
        /// <param name="reply">是否引用回复</param>
        /// <returns>任务</returns>
        public virtual Task ReplyAsync(List<object> message, bool reply = false)
        {
            if (_replyAsyncDelegate != null)
            {
                return _replyAsyncDelegate(message, reply);
            }
            throw new NotImplementedException("需要在OneBotService中实现此方法");
        }

        /// <summary>
        /// 回复文本消息
        /// </summary>
        /// <param name="message">文本消息</param>
        /// <param name="reply">是否引用回复</param>
        /// <returns>任务</returns>
        public virtual Task ReplyAsync(string message, bool reply = false)
        {
            if (_replyAsyncStringDelegate != null)
            {
                return _replyAsyncStringDelegate(message, reply);
            }
            throw new NotImplementedException("需要在OneBotService中实现此方法");
        }

        /// <summary>
        /// 判断消息是否@了机器人
        /// </summary>
        /// <returns>是否@了机器人</returns>
        public bool IsToMe()
        {
            foreach (var item in Message)
            {
                if (item is MessageModel<AtMessageDataModel> msg)
                {
                    if (msg.Data.Id == SelfId.ToString())
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 提取纯文本消息
        /// </summary>
        /// <returns>纯文本消息</returns>
        public string ExtractPlainText()
        {
            StringBuilder sb = new();
            foreach (var item in Message)
            {
                if (item is MessageModel<TextMessageDataModel> t)
                    sb.Append(t.Data.Text);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 获取被@的用户ID列表
        /// </summary>
        /// <returns>被@的用户ID列表</returns>
        public string[] GetMentioned()
        {
            var result = new List<string>();
            foreach (var item in Message)
            {
                if (item is MessageModel<AtMessageDataModel> msg && msg.Data.Id != SelfId.ToString())
                {
                    result.Add(msg.Data.Id);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// 获取回复的消息ID
        /// </summary>
        /// <returns>回复的消息ID</returns>
        public string GetReplyId()
        {
            foreach (var item in Message)
            {
                if (item is MessageModel<ReplyMessageDataModel> msg)
                {
                    return msg.Data.Id;
                }
            }
            return string.Empty;
        }

        public override string ToString()
        {
            return $"[{PostType}] {MessageType} {SubType} {UserId} : ({MessageId}) {RawMessage}";
        }
    }

    /// <summary>
    /// 发送者模型
    /// </summary>
    public class SenderModel
    {
        public long UserId { get; set; }
        public string? Nickname { get; set; }
        public string? Sex { get; set; }
        public int Age { get; set; }
        public string? Card { get; set; }
        public string? Area { get; set; }
        public string? Level { get; set; }
        public string? Role { get; set; }
        public string? Title { get; set; }
    }

    /// <summary>
    /// 群消息事件模型
    /// </summary>
    public class GroupMessageEventModel : MessageEventModel
    {
        // 用于存储委托的私有字段
        private Func<List<object>, bool, Task>? _replyAsyncDelegate = null;

        public long GroupId { get; init; }
        public AnonymousModel? Anonymous { get; init; }

        public override Task ReplyAsync(List<object> message, bool reply = false)
        {
            if (_replyAsyncDelegate != null)
            {
                return _replyAsyncDelegate(message, reply);
            }
            throw new NotImplementedException("需要在OneBotService中实现此方法");
        }

        // 确保字符串重载方法也能正确工作
        public override Task ReplyAsync(string message, bool reply = false)
        {
            // 创建文本消息对象
            var textMessage = new List<object>
            {
                new MessageModel<TextMessageDataModel>
                {
                    Type = "text",
                    Data = new TextMessageDataModel { Text = message }
                }
            };
            
            // 直接调用当前类的 ReplyAsync(List<object>, bool) 方法
            return this.ReplyAsync(textMessage, reply);
        }

        public override string ToString()
        {
            return $"[{PostType}] {MessageType} {SubType} {UserId} @ {GroupId} : ({MessageId}) {RawMessage}";
        }
    }

    /// <summary>
    /// 匿名用户模型
    /// </summary>
    public class AnonymousModel
    {
        public required string Flag { get; set; }
        public long Id { get; set; }
        public required string Name { get; set; }
    }
}