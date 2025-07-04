using QwenBotQ.NET.OneBot.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace QwenBotQ.NET.OneBot.Core
{
    internal partial class OneBot
    {
        private async Task HandleEvent(string data)
        {
            dynamic? eventModel = null;
            try
            {
                using (var doc = JsonDocument.Parse(data))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("post_type", out var postType))
                    {
                        switch (postType.GetString())
                        {
                            case "message":
                                if (root.TryGetProperty("message_type", out var msgType))
                                {
                                    switch (msgType.GetString())
                                    {
                                        case "private":
                                            eventModel = JsonSerializer.Deserialize<MessageEventModel>(data);
                                            break;
                                        case "group":
                                            eventModel = JsonSerializer.Deserialize<GroupMessageEventModel>(data);
                                            break;
                                        default:
                                            _logger.LogDebug($"Unhandled message type: {msgType}");
                                            break;
                                    }
                                }
                                else
                                {
                                    _logger.LogDebug("No message_type found in the data.");
                                    return;
                                }
                                break;
                            default:
                                _logger.LogWarning($"Unhandled post type: {postType}");
                                break;
                        }
                    }
                    else
                    {
                        _logger.LogDebug("No post_type found in the data.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling data: {ex.Message}");
            }
            if (eventModel != null)
            {
                eventModel.Bot = this;
                await OnEvent.Invoke(eventModel);
            }
        }
    }
}
