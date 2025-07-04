namespace QwenBotQ.NET.OneBot.Models
{
    public class ApiResponse
    {
        public required string Status { get; set; }
        public int Retcode { get; set; }
        public long Echo { get; set; }
        public object? Data { get; set; }
    }

    public class ApiResponse<T> : ApiResponse where T : BaseRespData
    {
        public new required T Data { get; set; }
    }

    public class MultiApiResponse<T> : ApiResponse where T : BaseRespData
    {
        public new required List<T> Data { get; set; }
    }

    public class BaseRespData { }

    public class GetStrangerInfoData : BaseRespData
    {
        public long UserId { get; set; }
        public required string Nickname { get; set; }
        public required string Sex { get; set; }
        public int Age { get; set; }
    }

    public class GetGroupMemberInfoData : BaseRespData
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