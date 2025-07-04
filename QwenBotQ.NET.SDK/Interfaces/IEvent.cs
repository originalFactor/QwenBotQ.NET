using System.Text.Json.Serialization;

namespace QwenBotQ.NET.SDK.Interfaces;

public interface IEvent
{
    [JsonPropertyName("time")]
    public long? Time { get; set; }

    [JsonPropertyName("self_id")]
    public long? SelfId { get; set; }

    [JsonPropertyName("post_type")]
    public string? PostType { get; set; }
}