using Newtonsoft.Json;
using JsonSubTypes;

namespace QwenBotQ.SDK.Models.OneBot.Events;

[JsonConverter(typeof(JsonSubtypes), "post_type")]
[JsonSubtypes.KnownSubType(typeof(MessageEvent), "message")]
[JsonSubtypes.KnownSubType(typeof(BaseNoticeEvent), "notice")]
[JsonSubtypes.KnownSubType(typeof(BaseMetaEvent), "meta_event")]
[JsonSubtypes.KnownSubType(typeof(AddFriendRequestEvent), "request")]
[JsonSubtypes.FallBackSubType(typeof(BaseEvent))]
public class BaseEvent
{
    public required long Time { get; set; }
    public required long SelfId { get; set; }
    public required string PostType { get; set; }
}
