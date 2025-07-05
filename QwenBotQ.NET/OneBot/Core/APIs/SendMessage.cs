using Microsoft.Extensions.Logging;
using QwenBotQ.NET.OneBot.Models;

namespace QwenBotQ.NET.OneBot.Core;

public partial class OneBot
{
    public async Task SendMessageAsync(List<object> message, long? userId = null, long? groupId = null)
    {
        var @params = new SendMsgParams
        {
            Message = message,
            UserId = userId,
            GroupId = groupId
        };

        if ((@params.UserId == null && @params.GroupId == null) || (@params.UserId != null && @params.GroupId != null))
        {
            _logger.LogWarning("Must specify one of userId and groupId");
            return;
        }
        await CallAsync<SendMsgParams>(
            "send_msg_rate_limited",
            @params
        );
    }

    public async Task SendMessageAsync(string message, long? userId = null, long? groupId = null)
    {
        var @params = new SendMsgParams
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
        if ((@params.UserId == null && @params.GroupId == null) || (@params.UserId != null && @params.GroupId != null))
        {
            _logger.LogWarning("Must specify one of userId and groupId");
            return;
        }
        await CallAsync<SendMsgParams>(
            "send_msg_rate_limited",
            @params
        );
    }
}
