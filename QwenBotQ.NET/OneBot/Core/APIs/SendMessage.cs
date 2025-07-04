using Microsoft.Extensions.Logging;
using QwenBotQ.NET.OneBot.Models;

namespace QwenBotQ.NET.OneBot.Core
{
    internal partial class OneBot
    {
        public async Task SendMessageAsync(List<object> message, long? userId = null, long? groupId = null)
        {
            var _params = new SendMsgParams
            {
                Message = message,
                UserId = userId,
                GroupId = groupId
            };

            if ((_params.UserId == null && _params.GroupId == null) || (_params.UserId != null && _params.GroupId != null))
            {
                _logger.LogWarning("Must specify one of userId and groupId");
                return;
            }
            await CallAsync<SendMsgParams>(
                "send_msg_rate_limited",
                _params
            );
        }

        public async Task SendMessageAsync(string message, long? userId = null, long? groupId = null)
        {
            var _params = new SendMsgParams
            {
                Message = new List<object>
                {
                    new Message<TextMessageData>
                    {
                        Type = "text",
                        Data = new TextMessageData
                        {
                            Text = message
                        }
                    }
                },
                UserId = userId,
                GroupId = groupId
            };
            if ((_params.UserId == null && _params.GroupId == null) || (_params.UserId != null && _params.GroupId != null))
            {
                _logger.LogWarning("Must specify one of userId and groupId");
                return;
            }
            await CallAsync<SendMsgParams>(
                "send_msg_rate_limited",
                _params
            );
        }
    }
}
