using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QwenBotQ.NET.OneBot.Models
{
    class ApiResponse
    {
        [JsonPropertyName("status")]
        public required string Status { get; set; }
        [JsonPropertyName("retcode")]
        public int RetCode { get; set; }
        [JsonPropertyName("echo")]
        public long Echo { get; set; }
        [JsonPropertyName("data")]
        public JsonElement? Data { get; set; }
    }

    class ApiResponse<T> : ApiResponse
    {
        [JsonPropertyName("data")]
        public new T? Data { get; set; }
    }

    class BaseRespData { }

    class GetStrangerInfoData : BaseRespData
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }
        [JsonPropertyName("nickname")]
        public required string Nickname { get; set; }
        [JsonPropertyName("sex")]
        public required string Sex { get; set; }
        [JsonPropertyName("age")]
        public int Age { get; set; }
    }

    class GetGroupMemberInfoData : BaseRespData
    {
        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }
        [JsonPropertyName("nickname")]
        public required string Nickname { get; set; }
        [JsonPropertyName("card")]
        public required string Card { get; set; }
        [JsonPropertyName("Sex")]
        public required string Sex { get; set; }
        [JsonPropertyName("age")]
        public int Age { get; set; }
        [JsonPropertyName("area")]
        public required string Area { get; set; }
        [JsonPropertyName("join_time")]
        public int JoinTime { get; set; }
        [JsonPropertyName("last_sent_time")]
        public int LastSendTime { get; set; }
        [JsonPropertyName("level")]
        public required string Level { get; set; }
        [JsonPropertyName("role")]
        public required string Role { get; set; }
        [JsonPropertyName("title")]
        public required string Title { get; set; }
        [JsonPropertyName("unfriendly")]
        public bool Unfriendly { get; set; }
        [JsonPropertyName("title_expire_time")]
        public int TitleExpireTime { get; set; }
        [JsonPropertyName("card_changeable")]
        public bool CardChangeable { get; set; }
    }
}