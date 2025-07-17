using QwenBotQ.SDK.Models.OneBot.Events;
using QwenBotQ.SDK.OneBotS;

namespace QwenBotQ.SDK.Context;

public class BaseContext 
{
    public required OneBot Bot { get; set; }
    public BaseEvent? Event { get; set; }
}
