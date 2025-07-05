using QwenBotQ.NET.Models;
using QwenBotQ.NET.Services.Interfaces;

namespace QwenBotQ.NET.Services;

public class ConfigService : IConfigService
{
    private readonly ConfigModel _config;

    public ConfigService()
    {
        _config = new ConfigModel();
    }

    public string OneBotServer => _config.OneBotServer;
    public string? OneBotToken => _config.OneBotToken;
    public string MongoUri => _config.MongoUri;
    public string MongoDbName => _config.MongoDbName;
}