using QwenBotQ.NET.OneBot.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace QwenBotQ.NET.OneBot.Core
{
    internal partial class OneBot
    {
        private async Task CallAsync<T>(string action, T args, Type? respType = null, Func<ApiResponse, Task>? callback = null) where T : BaseParams
        {
            var echo = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var data = JsonSerializer.Serialize(
                new ApiCall<T>
                {
                    Action = action,
                    Params = args,
                    Echo = echo
                }
            );
            _logger.LogInformation($"Sending API Call: {data}");
            try
            {
                await _client.SendAsync(data);
                if (respType != null && callback != null)
                    _api_waiting[echo] = new Tuple<Type, Func<ApiResponse, Task>>(respType, callback);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling action {action}: {ex.Message}");
            }
        }
    }
}
