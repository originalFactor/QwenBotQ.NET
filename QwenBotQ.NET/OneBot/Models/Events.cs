using JsonSubTypes;
using Newtonsoft.Json;
using System.Text;

namespace QwenBotQ.NET.OneBot.Models;

[JsonConverter(typeof(JsonSubtypes), "post_type")]
[JsonSubtypes.KnownSubType(typeof(MessageEventModel), "message")]
[JsonSubtypes.FallBackSubType(typeof(BaseEventModel))]
public class BaseEventModel
{
    public Core.OneBot? Bot { get; set; }
    public required string PostType { get; set; }
    public long Time { get; set; }
    public long SelfId { get; set; }
}

[JsonConverter(typeof(JsonSubtypes), "message_type")]
[JsonSubtypes.KnownSubType(typeof(GroupMessageEventModel), "group")]
[JsonSubtypes.FallBackSubType(typeof(MessageEventModel))]
public class MessageEventModel : BaseEventModel
{
    public required string MessageType { get; init; }
    public required string SubType { get; init; }
    public long UserId { get; init; }
    public long MessageId { get; init; }
    public required List<Message> Message { get; init; }
    public required string RawMessage { get; init; }
    public int Font { get; init; }
    public required SenderModel Sender { get; init; }

    public virtual async Task ReplyAsync(List<object> message, bool reply = false)
    {
        if(reply)
            message.Insert(0, new Message<ReplyMessageData>
            {
                Type = "reply",
                Data = new ReplyMessageData { Id = MessageId.ToString() }
            });
        if(Bot != null)
            await Bot.SendMessageAsync(message, UserId, null);
        else
            throw  new Exception("Bot is not initialized");
    }

    public async Task ReplyAsync(string message, bool reply = false)
    {
        await ReplyAsync(new List<object>
            {
                new Message<TextMessageData>
                {
                    Type = "text",
                    Data = new TextMessageData { Text = message }
                }
            },
            reply
        );
    }

    public bool IsToMe()
    {
        foreach (var item in Message)
        {
            if (item is Message<AtMessageData> msg)
            {
                if (msg.Data.Id == SelfId.ToString())
                    return true;
            }
        }

        return false;
    }

    public string ExtractPlainText()
    {
        StringBuilder sb = new();
        foreach(var item in Message)
        {
            if(item.Type == "text" && item.Data is TextMessageData data)
                sb.Append(data.Text);
        }
        
        return sb.ToString();
    }
}

public class SenderModel
{
    public long UserId { get; set; }
    public string? Nickname { get; set; }
    public string? Sex { get; set; }
    public int Age { get; set; }
    public string? Card { get; set; }
    public string? Area { get; set; }
    public string? Level { get; set; }
    public string? Role { get; set; }
    public string? Title { get; set; }
}

public class GroupMessageEventModel : MessageEventModel
{
    public long GroupId { get; init; }
    public Annoymous? Anonymous { get; init; }

    public override async Task ReplyAsync(List<object> message, bool reply = false)
    {
        
        if(Bot == null) throw new Exception("Bot is not initialized");
        if(reply)
            message.Insert(0, new Message<ReplyMessageData>
            {
                Type = "reply",
                Data = new ReplyMessageData { Id = MessageId.ToString() }
            });
        message.Insert(0, new Message<AtMessageData>
        {
            Type = "at",
            Data = new AtMessageData
            {
                Id = UserId.ToString()
            }
        });
        await Bot.SendMessageAsync(message, null, GroupId);
    }
}

public class Annoymous
{
    public required string Flag { get; set; }
    public long Id { get; set; }
    public required string Name { get; set; }
}
