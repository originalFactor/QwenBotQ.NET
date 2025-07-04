using Microsoft.Extensions.Logging;

namespace QwenBotQ.NET.OneBot.Core
{
    internal partial class OneBot
    {
        public async Task ConnectAsync()
        {
            _logger.LogInformation("Connecting to OneBot server...");
            await _client.ConnectAsync();
            _logger.LogInformation("Connected to OneBot server.");
        }

        public async Task CloseAsync()
        {
            _logger.LogInformation("Closing OneBot connection...");
            await _client.DisconnectAsync();
            _logger.LogInformation("OneBot connection closed.");
        }
    }
}
