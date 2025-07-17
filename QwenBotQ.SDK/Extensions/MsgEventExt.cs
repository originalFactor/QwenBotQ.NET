using QwenBotQ.SDK.Messages;
using QwenBotQ.SDK.Models.OneBot.Events;
using QwenBotQ.SDK.Models.OneBot.Messages;

namespace QwenBotQ.SDK.Extensions;

public static class MessageEventExtensions
{
    public static string?[] GetMentionedArray(this Message msg)
    {
        return msg
            .OfType<Segment<AtData>>()
            .Select(segment => segment.Data?.Qq)
            .ToArray();
    }

    public static string GetPlainText(this Message msg)
    {
        return string.Join("", msg
            .OfType<Segment<TextData>>()
            .Select(segment => segment.Data?.Text));
    }

    public static bool IsToMe(this MessageEvent messageEvent)
    {
        return messageEvent.Message
            .GetMentionedArray()
            .Contains(messageEvent.UserId.ToString());
    }
}
