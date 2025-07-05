using Newtonsoft.Json;

namespace QwenBotQ.NET.OneBot.Models;

public class ApiCall
{
    public required string Action { get; set; }
    public object? Params { get; set; }
    public long Echo { get; set; }
}

public class ApiCall<T> : ApiCall where T : BaseParams
{
    public new required T Params { get; set; }
}

public class BaseParams { }

public class SendMsgParams : BaseParams
{
    public long? UserId { get; set; }
    public long? GroupId { get; set; }
    public required List<object> Message { get; set; }
}

public class GetStrangerInfoParams : BaseParams
{
    public long UserId { get; set; }
}

public class GetGroupMemberInfoParams : GetGroupMemberListParams
{
    public long UserId { get; set; }
}

public class GetGroupMemberListParams : BaseParams
{
    public long GroupId { get; set; }
}
