using Newtonsoft.Json;

namespace QwenBotQ.NET.Models.OneBot
{
    /// <summary>
    /// API调用基类
    /// </summary>
    public class ApiModel
    {
        public required string Action { get; set; }
        public object? Params { get; set; }
        public long Echo { get; set; }
    }

    /// <summary>
    /// 泛型API调用类
    /// </summary>
    /// <typeparam name="T">参数类型</typeparam>
    public class ApiModel<T> : ApiModel where T : BaseParamsModel
    {
        public new required T Params { get; set; }
    }

    /// <summary>
    /// API参数基类
    /// </summary>
    public class BaseParamsModel { }

    /// <summary>
    /// 发送消息参数
    /// </summary>
    public class SendMessageParamsModel : BaseParamsModel
    {
        public long? UserId { get; set; }
        public long? GroupId { get; set; }
        public required List<object> Message { get; set; }
    }

    /// <summary>
    /// 获取陌生人信息参数
    /// </summary>
    public class GetStrangerInfoParamsModel : BaseParamsModel
    {
        public long UserId { get; set; }
    }

    /// <summary>
    /// 获取群成员信息参数
    /// </summary>
    public class GetGroupMemberInfoParamsModel : GetGroupMemberListParamsModel
    {
        public long UserId { get; set; }
    }

    /// <summary>
    /// 获取群成员列表参数
    /// </summary>
    public class GetGroupMemberListParamsModel : BaseParamsModel
    {
        public long GroupId { get; set; }
    }
}