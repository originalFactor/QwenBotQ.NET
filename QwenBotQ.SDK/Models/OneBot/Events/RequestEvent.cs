using Newtonsoft.Json;
using JsonSubTypes;

namespace QwenBotQ.SDK.Models.OneBot.Events;

[JsonConverter(typeof(JsonSubtypes), "request_type")]
[JsonSubtypes.KnownSubType(typeof(JoinGroupRequestEvent), "group")]
[JsonSubtypes.FallBackSubType(typeof(AddFriendRequestEvent))]
public class AddFriendRequestEvent : BaseEvent
{
    public required string RequestType { get; set; }
    public required long UserId { get; set; }
    public required string Comment { get; set; }
    public required string Flag { get; set; }
}

public class JoinGroupRequestEvent : AddFriendRequestEvent
{
    public required long GroupId { get; set; }
    public required string SubType { get; set; }
}
