using QwenBotQ.SDK.Models.OneBot.Events;
using QwenBotQ.SDK.Messages;
using QwenBotQ.SDK.Models.OneBot.API;
using QwenBotQ.SDK.Models.OneBot.Messages;
using System.Linq;
namespace QwenBotQ.SDK.Context;


public class MessageContext : BaseContext
{
    public new MessageEvent? Event
    {
        get => (MessageEvent?)base.Event;
        set => base.Event = value;
    }
    public virtual async Task Quick(object reply)
    {
        if(reply is not Message && reply is not string)
        {
            throw new ArgumentException("Reply must be a Message or CQ string.", nameof(reply));
        }
        if (Event == null) throw new InvalidOperationException("Event is not set for this context.");
        await Bot.QuickOperationAsync(Event, new { reply });
    }
    public async Task<GetMessageData[]> TrackRepliesAsync()
    {
        var replies = new List<GetMessageData>();
        var curr = Event?.MessageId ?? 0;
        while(curr != 0)
        {
            var msg = await Bot.GetMsgAsync(curr);
            if (msg.Data?.Sender.UserId == null) break;
            replies.Add(msg.Data);
            var next = msg.Data.Message.FirstOrDefault<Segment<IdOnlyData>>("reply");
            curr = Convert.ToInt32(next?.Data?.Id);
        }
        return replies.ToArray();
    }
}

public class GroupMessageContext : MessageContext
{
    public new GroupMessageEvent? Event 
    {
        get => (GroupMessageEvent?)base.Event;
        set => base.Event = value;
    }

    public override async Task Quick(object reply) => await Quick(reply, true);
    public async Task Quick(
        object? reply = null,
        bool? atSender = null,
        bool? delete = null,
        bool? kick = null,
        bool? ban = null,
        int? banDuration = null
        )
    {
        if (Event == null) throw new InvalidOperationException("Event is not set for this context.");
        if (reply != null)
        {
            if(reply is not Message && reply is not string)
                throw new ArgumentException("Reply must be a Message or CQ string.", nameof(reply));
            if(atSender == true)
            {
                if(reply is Message msg)
                {
                    reply = new Message("\n") + msg;
                }
                else if(reply is string str)
                {
                    reply = "\n" + str;
                }
            }
        }
        await Bot.QuickOperationAsync(
            Event,
            new
            {
                reply = reply,
                at_sender = atSender,
                delete = delete,
                kick = kick,
                ban = ban,
                ban_duration = banDuration
            }
        );
    }
}