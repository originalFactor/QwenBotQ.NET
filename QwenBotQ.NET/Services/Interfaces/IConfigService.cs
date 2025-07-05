namespace QwenBotQ.NET.Services.Interfaces;

public interface IConfigService
{
    string OneBotServer { get; }
    string? OneBotToken { get; }
    string MongoUri { get; }
    string MongoDbName { get; }
}