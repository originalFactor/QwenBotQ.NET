using JsonSubTypes;
using Newtonsoft.Json;

namespace QwenBotQ.NET.OneBot.Models;

[JsonConverter(typeof(JsonSubtypes), "type")]
[JsonSubtypes.KnownSubType(typeof(Message<TextMessageData>), "text")]
[JsonSubtypes.KnownSubType(typeof(Message<AtMessageData>), "at")]
[JsonSubtypes.KnownSubType(typeof(Message<ReplyMessageData>), "reply")]
[JsonSubtypes.FallBackSubType(typeof(Message))]
public class Message
{
    public required string Type { get; init; }
    public object? Data { get; set; }
}

public class Message<T> : Message where T : BaseMessageData
{
    public new required T Data { get; set; }
}

public class BaseMessageData { }

public class TextMessageData : BaseMessageData
{
    public required string Text { get; set; }
}

public class ReplyMessageData : BaseMessageData
{
    public required string Id { get; set; }
}

public class AtMessageData : BaseMessageData
{
    [JsonProperty("qq")]
    public required string Id { get; set; }
}

