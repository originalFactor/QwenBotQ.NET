using Microsoft.Extensions.Logging;
using QwenBotQ.NET.OneBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace QwenBotQ.NET.OneBot.Core
{
    public partial class OneBot
    {
        async Task HandleApiResponse(string data)
        {
            try
            {
                var doc = JObject.Parse(data);
                if (doc.TryGetValue("echo", out var echo))
                {
                    long echoValue = echo.Value<long>();
                    if (_api_waiting.TryGetValue(echoValue, out var tuple))
                    {
                        var resp = (ApiResponse?)JsonConvert.DeserializeObject(data, tuple.Item1);
                        if (resp != null)
                            await tuple.Item2.Invoke(resp);
                    }
                }
                else
                {
                    _logger.LogDebug("No echo found in the data.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling API response: {ex.Message}");
            }
        }
    }
}
