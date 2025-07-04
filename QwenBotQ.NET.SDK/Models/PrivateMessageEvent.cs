using System.Text.Json.Serialization;
using QwenBotQ.NET.SDK.Interfaces;

namespace QwenBotQ.NET.SDK.Models;

public class PrivateMessageEvent : IMessageEvent
{
    [JsonPropertyName("time")]
    public long? Time { get; set; }

    [JsonPropertyName("self_id")]
    public long? SelfId { get; set; }

    [JsonPropertyName("post_type")]
    public string? PostType { get; set; }

    [JsonPropertyName("message_type")]
    public string? MessageType { get; set; }

    [JsonPropertyName("sub_type")]
    public string? SubType { get; set; }

    [JsonPropertyName("message_id")]
    public int? MessageId { get; set; }

    [JsonPropertyName("user_id")]
    public long? UserId { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("raw_message")]
    public string? RawMessage { get; set; }

    [JsonPropertyName("font")]
    public int? Font { get; set; }
}