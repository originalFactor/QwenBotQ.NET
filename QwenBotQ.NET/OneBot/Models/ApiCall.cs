using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QwenBotQ.NET.OneBot.Models
{
    class ApiCall
    {
        [JsonPropertyName("action")]
        public required string Action { get; set; }

        [JsonPropertyName("params")]
        public JsonElement? Params { get; set; }
        [JsonPropertyName("echo")]
        public long Echo { get; set; }
    }

    class ApiCall<T> : ApiCall where T : BaseParams
    {
        [JsonPropertyName("params")]
        public new required T Params { get; set; }
    }

    class BaseParams { }

    class SendMsgParams : BaseParams
    {
        [JsonPropertyName("user_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? UserId { get; set; }
        [JsonPropertyName("group_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? GroupId { get; set; }
        [JsonPropertyName("message")]
        public required List<object> Message { get; set; }
    }

    class GetStrangerInfoParams : BaseParams
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }
    }

    class GetGroupMemberInfoParams : GetGroupMemberListParams
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }
    }
    
    class GetGroupMemberListParams : BaseParams
    {
        [JsonPropertyName("group_id")]
        public long GroupId { get; set; }
    }
}
