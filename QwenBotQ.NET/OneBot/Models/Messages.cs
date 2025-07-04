using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace QwenBotQ.NET.OneBot.Models
{
    class Message
    {
        [JsonPropertyName("type")]
        public required string Type { get; set; }
        [JsonPropertyName("data")]
        public JsonElement? Data { get; set; }
    }

    class Message<T> : Message where T : BaseMessageData
    {
        [JsonPropertyName("data")]
        public new required T Data { get; set; }
    }

    class BaseMessageData { }

    class TextMessageData : BaseMessageData
    {
        [JsonPropertyName("text")]
        public required string Text { get; set; }
    }

    class ReplyMessageData : BaseMessageData
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }
    }
    
    class AtMessageData : BaseMessageData
    {
        [JsonPropertyName("qq")]
        public required string Id { get; set; }
    }
}
