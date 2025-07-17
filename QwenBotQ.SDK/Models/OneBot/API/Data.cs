using QwenBotQ.SDK.Messages;
using QwenBotQ.SDK.Models.OneBot.Global;
using QwenBotQ.SDK.Models.OneBot.Messages;

namespace QwenBotQ.SDK.Models.OneBot.API;

public class BaseData { }

public class SendMessageData : BaseData
{
    public required int MessageId { get; set; }
}

public class GetMessageData : BaseData
{
    public required Message Message { get; set; }
    public required int MessageId { get; set; }
    public required int RealId { get; set; }
    public required Sender Sender { get; set; }
    public required string MessageType { get; set; }
    public required int Time { get; set; }
}

public class ForwardMessageData : BaseData
{
    public required Message<Messages.CustomNodeData> Message { get; set; }
}

public class LoginInfoData : BaseData
{
    public required long UserId { get; set; }
    public required string Nickname { get; set; }
}

public class StrangerInfoData : BaseData
{
    public required long UserId { get; set; }
    public required string Nickname { get; set; }
    public required string Sex { get; set; }
    public required int Age { get; set; }
}

public class FriendListData : BaseData
{
    public required long UserId { get; set; }
    public required string Nickname { get; set; }
    public required string Remark { get; set; }
}

public class GroupInfoData : BaseData
{
    public required long GroupId { get; set; }
    public required string GroupName { get; set; }
    public required int MemberCount { get; set; }
    public required int MaxMemberCount { get; set; }
}

public class GroupMemberInfoData : BaseData
{
    public required long UserId { get; set; }
    public required string Nickname { get; set; }
    public required string Card { get; set; }
    public required string Sex { get; set; }
    public required int Age { get; set; }
    public required string Area { get; set; }
    public required string JoinTime { get; set; }
    public required string LastSentTime { get; set; }
    public required string Level { get; set; }
    public required string Role { get; set; }
    public required bool Unfriendly { get; set; }
    public required string Title { get; set; }
    public required int TitleExpireTime { get; set; }
    public required bool CardChangeable { get; set; }
}

public class GroupHonorInfoData : BaseData
{
    public required long GroupId { get; set; }
    public CurrentTalkactive? CurrentTalkactive { get; set; }
    public List<HonorData>? TalkactiveList { get; set; }
    public List<HonorData>? PerformerList { get; set; }
    public List<HonorData>? LegendList { get; set; }
    public List<HonorData>? StrongNewbieList { get; set; }
    public List<HonorData>? EmotionList { get; set; }
}

public class CurrentTalkactive
{
    public required long UserId { get; set; }
    public required string Nickname { get; set; }
    public required string Avatar { get; set; }
    public required int DayCount { get; set; }
}

public class HonorData
{
    public required long UserId { get; set; }
    public required string Nickname { get; set; }
    public required string Avatar { get; set; }
    public required string Description { get; set; }
}


public class CredentialsData : BaseData 
{ 
    public required string Cookies { get; set; }
    public required int CsrfToken { get; set; }
}

public class FileData : BaseData
{
    public required string File { get; set; }
}

public class AvailabilityData : BaseData
{
    public required bool Yes { get; set; }
}

public class  StatusData : BaseData
{
    public bool? Online { get; set; }
    public required bool Good { get; set; }
}

public class VersionData : BaseData
{
    public required string AppName { get; set; }
    public required string AppVersion { get; set; }
    public required string ProtocolVersion { get; set; }
}
