using MongoDB.Bson.Serialization.Attributes;

namespace QwenBotQ.NET.Models;

public class UserModel
{
    public required string Id { get; set; }
    public required string Nick { get; set; }
    public int Permission { get; set; } = 0; // 0: normal, 1: admin, 2: owner, 3: global owner
    public string SystemPrompt { get; set; } = "You are a helpful assistant";
    public double Temprature { get; set; } = 1.0;
    public double FrequencyPenalty { get; set; } = 0.0;
    public double PresencePenalty { get; set; } = 0.0;
    public long Coins { get; set; } = 0;
    public DateOnly SignExpire { get; set; } = DateOnly.MinValue;
    public string? model { get; set; }
    public DateOnly ProfileExpire { get; set; } = DateOnly.MinValue;
    public double BindPower { get; set; } = 0.0;
    public BindedModel? Binded { get; set; }
}

public class BindedModel
{
    [BsonElement("id")]
    public required string Ident { get; set; }
    public DateTime Expire { get; set; } = DateTime.MinValue;
}