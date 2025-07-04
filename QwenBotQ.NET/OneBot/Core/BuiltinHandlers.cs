using Microsoft.Extensions.Logging;
using QwenBotQ.NET.OneBot.Models;

namespace QwenBotQ.NET.OneBot.Core
{
    internal partial class OneBot
    {
        private async Task BuiltinHandler(BaseEventModel model)
        {
            await Task.Run(() =>
            {
                if (model is MessageEventModel msg)
                {
                    if (model is GroupMessageEventModel group)
                        _logger.LogInformation($"From {group.GroupId}:");
                    _logger.LogInformation($"{msg.Sender.Nickname} ({msg.UserId}): {msg.RawMessage}");
                }
                else
                {
                    _logger.LogInformation($"Received event of type {model.GetType().Name} at {model.Time}");
                }
            });
        }
    }
        
}
