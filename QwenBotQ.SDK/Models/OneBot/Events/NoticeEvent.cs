using Newtonsoft.Json;
using JsonSubTypes;

namespace QwenBotQ.SDK.Models.OneBot.Events;

[JsonConverter(typeof(JsonSubtypes), "notice_type")]
[JsonSubtypes.KnownSubType(typeof(GroupUploadEvent), "group_upload")]
[JsonSubtypes.KnownSubType(typeof(GroupEventWithSubType), "group_admin")]
[JsonSubtypes.KnownSubType(typeof(GroupEventWithSubTypeAndOperator), "group_decrease")]
[JsonSubtypes.KnownSubType(typeof(GroupEventWithSubTypeAndOperator), "group_increase")]
[JsonSubtypes.KnownSubType(typeof(GroupBanEvent), "group_ban")]
[JsonSubtypes.KnownSubType(typeof(UserEvent), "friend_add")]
[JsonSubtypes.KnownSubType(typeof(GroupRecallEvent), "group_recall")]
[JsonSubtypes.KnownSubType(typeof(FriendRecallEvent), "friend_recall")]
[JsonSubtypes.KnownSubType(typeof(GroupEventWithSubType), "notify")]
[JsonSubtypes.FallBackSubType(typeof(BaseNoticeEvent))]
public class BaseNoticeEvent : BaseEvent
{
    public required string NoticeType { get; set; }
}

public class UserEvent : BaseNoticeEvent
{
    public required long UserId { get; set; }
}

public class GroupEvent : UserEvent
{
    public required long GroupId { get; set; }
}

[JsonConverter(typeof(JsonSubtypes), "sub_type")]
[JsonSubtypes.KnownSubType(typeof(GroupHonorEvent), "honor")]
[JsonSubtypes.KnownSubType(typeof(GroupTargetedEvent), "lucky_king")]
[JsonSubtypes.KnownSubType(typeof(GroupTargetedEvent), "poke")]
[JsonSubtypes.FallBackSubType(typeof(GroupEventWithSubType))]
public class GroupEventWithSubType : GroupEvent
{
    public required string SubType { get; set; }
}

public class GroupEventWithOperator : GroupEvent
{
    public required long OperatorId { get; set; }
}

public class GroupEventWithSubTypeAndOperator : GroupEventWithSubType
{
    public required long OperatorId { get; set; }
}

public class GroupUploadEvent : GroupEvent
{
    public required File File { get; set; }
}

public class File
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required long Size { get; set; }
    public required long Busid { get; set; }
}

public class GroupBanEvent : GroupEventWithSubTypeAndOperator
{
    public required long Duration { get; set; }
}

public class GroupRecallEvent : GroupEventWithSubType
{
    public required long MessageId { get; set; }
}

public class FriendRecallEvent : UserEvent
{
    public required long MessageId { get; set; }
}

public class GroupTargetedEvent : GroupEventWithSubType
{
    public required long TargetId { get; set; }
}

class GroupHonorEvent : GroupEventWithSubType
{
    public required string HonorType { get; set; }
}