using Microsoft.Extensions.Logging;
using QwenBotQ.NET.OneBot.Models;

namespace QwenBotQ.NET.OneBot.Core
{
    public partial class OneBot
    {
        private async Task BuiltinHandler(BaseEventModel model)
        {
            await Task.Run(() =>
            {
                if (model is MessageEventModel msg)
                {
                    string from = "private";
                    if (model is GroupMessageEventModel group)
                        from = group.GroupId.ToString();
                    _logger.LogInformation($"{msg.Sender.Nickname} ({msg.UserId} from {from}): {msg.RawMessage}");
                }
                else
                {
                    _logger.LogInformation($"Received event of type {model.PostType} at {model.Time}");
                }
            });
        }
    }
        
}
