using Newtonsoft.Json;
using JsonSubTypes;
namespace QwenBotQ.SDK.Models.OneBot.Events;

[JsonConverter(typeof(JsonSubtypes), "meta_event_type")]
[JsonSubtypes.KnownSubType(typeof(LifecycleEvent), "lifecycle")]
[JsonSubtypes.KnownSubType(typeof(HeartbeatEvent), "heartbeat")]
[JsonSubtypes.FallBackSubType(typeof(BaseMetaEvent))]
public class BaseMetaEvent : BaseEvent
{
    public required string MetaEventType { get; set; }
}

public class LifecycleEvent : BaseMetaEvent
{
    public required string SubType { get; set; }
}

public class HeartbeatEvent : BaseMetaEvent
{
    public required Status Status { get; set; }
    public required long Interval { get; set; }
}

public class Status
{
    public bool? Online { get; set; }
    public required bool Good { get; set; }
}
