using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using QwenBotQ.NET.OneBot.Models;
using JsonSubTypes;
using Microsoft.Extensions.Logging;
using System;

namespace QwenBotQ.NET.OneBot.Core;

public partial class OneBot
{
    private async Task HandleEvent(string data)
    {
        _logger.LogDebug($"Received event data: {data}");
        try
        {
            var eventModel = JsonConvert.DeserializeObject<BaseEventModel>(data);
            _logger.LogDebug($"Deserialized event type: {eventModel?.GetType().Name}, PostType: {eventModel?.PostType}");
            
            if(eventModel!=null)
            {
                eventModel.Bot = this;
                await OnEvent.Invoke(eventModel);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deserializing event: {ex.Message}");
            _logger.LogError($"Event data: {data}");
        }
    }
}
