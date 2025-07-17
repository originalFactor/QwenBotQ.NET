using QwenBotQ.SDK.Models.OneBot.Messages;

namespace QwenBotQ.SDK.Messages;

public class Message<T> : List<Segment<T>> where T : BaseData
{
    public Message() : base() { }
    public Message(IEnumerable<Segment<T>> segments) : base(segments) { }
    public Message(params Segment<T>[] segments) : base(segments) { }
    public Message(IEnumerable<Segment> segments) : base(segments.OfType<Segment<T>>()) { }
    public Message(params Segment[] segments) : this(segments.AsEnumerable()) { }
    public static Message<T> operator +(Message<T> message, Segment<T> segment)
    {
        message.Add(segment);
        return message;
    }
    public static Message<T> operator +(Message<T> message, IEnumerable<Segment<T>> segments)
    {
        message.AddRange(segments);
        return message;
    }
    public static Message<T> operator +(Message<T> message, IEnumerable<Segment> segments)
    {
        message.AddRange(segments.OfType<Segment<T>>());
        return message;
    }
}

public class Message : List<Segment>
{
    public Message() : base() { }
    public Message(IEnumerable<Segment> segments) : base(segments) { }
    public Message(params Segment[] segments) : base(segments) { }
    public Message(IEnumerable<string> data) : base(data.Select(d => new Segment<TextData> { Type = "text", Data = new TextData { Text = d } })) { }
    public Message(params string[] data) : this(data.AsEnumerable()) { }
    public static Message operator +(Message message, Segment segment)
    {
        message.Add(segment);
        return message;
    }
    public static Message operator +(Message message, IEnumerable<Segment> segments)
    {
        message.AddRange(segments);
        return message;
    }

    public T? FirstOrDefault<T>(string? type = null) where T : Segment
    {
        var t = this.OfType<T>();
        if (type != null)
        {
            t = t.Where(s => s.Type == type);
        }
        return t.FirstOrDefault();
    }
}