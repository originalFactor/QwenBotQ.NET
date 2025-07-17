using JsonSubTypes;
using Newtonsoft.Json;
using QwenBotQ.SDK.Messages;
using QwenBotQ.SDK.Models.OneBot.Global;

namespace QwenBotQ.SDK.Models.OneBot.Events;

[JsonConverter(typeof(JsonSubtypes), "message_type")]
[JsonSubtypes.KnownSubType(typeof(GroupMessageEvent), "group")]
[JsonSubtypes.FallBackSubType(typeof(MessageEvent))]
public class MessageEvent : BaseEvent
{
    public required string MessageType { get; set; }
    public required string SubType { get; set; }
    public required int MessageId { get; set; }
    public required long UserId { get; set; }
    public required Message Message { get; set; }
    public required string RawMessage { get; set; }
    public required int Font { get; set; }
    public required Sender Sender { get; set; }
}

public class GroupMessageEvent : MessageEvent
{
    public required long GroupId { get; set; }
    public Anonymous? Anonymous { get; set; }
}

public class Anonymous
{
    public required long Id { get; set; }
    public required string Name { get; set; }
    public required string Flag { get; set; }
}