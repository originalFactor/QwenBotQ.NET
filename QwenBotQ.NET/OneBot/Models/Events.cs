using System.Text.Json.Serialization;

namespace QwenBotQ.NET.OneBot.Models
{
    class BaseEventModel
    {
        public Core.OneBot? Bot { get; set; }
        [JsonPropertyName("post_type")]
        public required string PostType { get; set; }
        [JsonPropertyName("time")]
        public long Time { get; set; }
        [JsonPropertyName("self_id")]
        public long SelfId { get; set; }
    }

    class MessageEventModel : BaseEventModel
    {
        [JsonPropertyName("message_type")]
        public required string MessageType { get; init; }
        [JsonPropertyName("sub_type")]
        public required string SubType { get; init; }
        [JsonPropertyName("user_id")]
        public long UserId { get; init; }
        [JsonPropertyName("message_id")]
        public long MessageId { get; init; }
        [JsonPropertyName("message")]
        public required List<Message> Message { get; init; }
        [JsonPropertyName("raw_message")]
        public required string RawMessage { get; init; }
        [JsonPropertyName("font")]
        public int Font { get; init; }
        [JsonPropertyName("sender")]
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
    }

    class SenderModel
    {
        [JsonPropertyName("user_id")]
        public long UserId { get; set; }
        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; }
        [JsonPropertyName("sex")]
        public string? Sex { get; set; }
        [JsonPropertyName("age")]
        public int Age { get; set; }
        [JsonPropertyName("card")]
        public string? GroupNick { get; set; }
        [JsonPropertyName("area")]
        public string? Area { get; set; }
        [JsonPropertyName("level")]
        public string? Level { get; set; }
        [JsonPropertyName("role")]
        public string? Role { get; set; }
        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }

    class GroupMessageEventModel : MessageEventModel
    {
        [JsonPropertyName("group_id")]
        public long GroupId { get; init; }
        [JsonPropertyName("anonymous")]
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

    class Annoymous
    {
        [JsonPropertyName("flag")]
        public required string Flag { get; set; }
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }
    }
}
