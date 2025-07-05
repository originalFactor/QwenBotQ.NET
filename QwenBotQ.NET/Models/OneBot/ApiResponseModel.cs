namespace QwenBotQ.NET.Models.OneBot
{
    /// <summary>
    /// API响应基类
    /// </summary>
    public class ApiResponseModel
    {
        public required string Status { get; set; }
        public int Retcode { get; set; }
        public long Echo { get; set; }
        public object? Data { get; set; }
    }

    /// <summary>
    /// 泛型API响应类
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class ApiResponseModel<T> : ApiResponseModel where T : BaseResponseDataModel
    {
        public new required T Data { get; set; }
    }

    /// <summary>
    /// 多结果API响应类
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class MultiApiResponseModel<T> : ApiResponseModel where T : BaseResponseDataModel
    {
        public new required List<T> Data { get; set; }
    }

    /// <summary>
    /// 响应数据基类
    /// </summary>
    public class BaseResponseDataModel { }

    /// <summary>
    /// 获取陌生人信息响应数据
    /// </summary>
    public class GetStrangerInfoDataModel : BaseResponseDataModel
    {
        public long UserId { get; set; }
        public required string Nickname { get; set; }
        public required string Sex { get; set; }
        public int Age { get; set; }
    }

    /// <summary>
    /// 获取群成员信息响应数据
    /// </summary>
    public class GetGroupMemberInfoDataModel : BaseResponseDataModel
    {
        public long GroupId { get; set; }
        public long UserId { get; set; }
        public required string Nickname { get; set; }
        public required string Card { get; set; }
        public required string Sex { get; set; }
        public int Age { get; set; }
        public required string Area { get; set; }
        public int JoinTime { get; set; }
        public int LastSentTime { get; set; }
        public required string Level { get; set; }
        public required string Role { get; set; }
        public required string Title { get; set; }
        public bool Unfriendly { get; set; }
        public int TitleExpireTime { get; set; }
        public bool CardChangeable { get; set; }
    }
}