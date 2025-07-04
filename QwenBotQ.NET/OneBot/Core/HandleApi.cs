using Microsoft.Extensions.Logging;
using QwenBotQ.NET.OneBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace QwenBotQ.NET.OneBot.Core
{
    internal partial class OneBot
    {
        async Task HandleApiResponse(string data)
        {
            try
            {
                using (var doc = JsonDocument.Parse(data))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("echo", out var echo))
                    {
                        if (echo.TryGetInt64(out var echoValue))
                        {
                            if (_api_waiting.TryGetValue(echoValue, out var tuple))
                            {
                                var resp = (ApiResponse?)JsonSerializer.Deserialize(doc, tuple.Item1);
                                if (resp != null)
                                    await tuple.Item2.Invoke(resp);
                            }
                            else
                            {
                                _logger.LogDebug($"No waiting API response for echo {echoValue}");
                            }
                        }
                        else
                        {
                            _logger.LogDebug($"Echo value is not a valid long: {echo}");
                        }
                    }
                    else
                    {
                        _logger.LogDebug("No echo found in the data.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling API response: {ex.Message}");
            }
        }
    }
}
