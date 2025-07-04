using Microsoft.Extensions.Logging;
using QwenBotQ.NET.OneBot.Models;
using QwenBotQ.NET.WebSocket;

namespace QwenBotQ.NET.OneBot.Core
{
    internal partial class OneBot
    {
        ILogger _logger;
        WebSocketClient _client;
        public event Func<BaseEventModel, Task> OnEvent;
        Dictionary<long, Tuple<Type, Func<ApiResponse, Task>>> _api_waiting;

        public OneBot(string? uri = null, string? token = null, ILoggerFactory? loggerFactory = null)
        {
            var loggerFactoryInstance = loggerFactory ?? LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactoryInstance.CreateLogger<OneBot>();
            _client = new WebSocketClient(uri, token, loggerFactoryInstance.CreateLogger<WebSocketClient>());
            _client.WSEvent += HandleEvent;
            OnEvent += BuiltinHandler;
            _api_waiting = new Dictionary<long, Tuple<Type, Func<ApiResponse, Task>>>();
        }
    }
}
