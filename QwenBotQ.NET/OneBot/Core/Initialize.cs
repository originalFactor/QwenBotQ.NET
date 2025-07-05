using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QwenBotQ.NET.OneBot.Models;
using QwenBotQ.NET.WebSocket;

namespace QwenBotQ.NET.OneBot.Core;

public partial class OneBot
{
    ILogger _logger;
    WebSocketClient _client;
    event Func<BaseEventModel, Task> OnEvent;
    Dictionary<long, Tuple<Type, Func<ApiResponse, Task>>> _api_waiting;

    public OneBot(string? uri = null, string? token = null, ILoggerFactory? loggerFactory = null)
    {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            },
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
        };
        
        var loggerFactoryInstance = loggerFactory ?? LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactoryInstance.CreateLogger<OneBot>();
        _client = new WebSocketClient(uri, token, loggerFactoryInstance.CreateLogger<WebSocketClient>());
        _client.WSEvent += HandleEvent;
        _client.WSEvent += HandleApiResponse;
        OnEvent += BuiltinHandler;
        _api_waiting = new Dictionary<long, Tuple<Type, Func<ApiResponse, Task>>>();
    }
}
