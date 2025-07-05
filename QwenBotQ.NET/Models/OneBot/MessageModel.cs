using JsonSubTypes;
using Newtonsoft.Json;

namespace QwenBotQ.NET.Models.OneBot
{
    /// <summary>
    /// 消息模型基类
    /// </summary>
    [JsonConverter(typeof(JsonSubtypes), "type")]
    [JsonSubtypes.KnownSubType(typeof(MessageModel<TextMessageDataModel>), "text")]
    [JsonSubtypes.KnownSubType(typeof(MessageModel<AtMessageDataModel>), "at")]
    [JsonSubtypes.KnownSubType(typeof(MessageModel<ReplyMessageDataModel>), "reply")]
    [JsonSubtypes.FallBackSubType(typeof(MessageModel))]
    public class MessageModel
    {
        public required string Type { get; init; }
        public object? Data { get; set; }
    }

    /// <summary>
    /// 泛型消息模型
    /// </summary>
    /// <typeparam name="T">消息数据类型</typeparam>
    public class MessageModel<T> : MessageModel where T : BaseMessageDataModel
    {
        public new required T Data { get; set; }
    }

    /// <summary>
    /// 消息数据基类
    /// </summary>
    public class BaseMessageDataModel { }

    /// <summary>
    /// 文本消息数据
    /// </summary>
    public class TextMessageDataModel : BaseMessageDataModel
    {
        public required string Text { get; set; }
    }

    /// <summary>
    /// 回复消息数据
    /// </summary>
    public class ReplyMessageDataModel : BaseMessageDataModel
    {
        public required string Id { get; set; }
    }

    /// <summary>
    /// @消息数据
    /// </summary>
    public class AtMessageDataModel : BaseMessageDataModel
    {
        [JsonProperty("qq")]
        public required string Id { get; set; }
    }
}