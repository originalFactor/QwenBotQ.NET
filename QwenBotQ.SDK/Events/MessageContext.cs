using QwenBotQ.SDK.Models;

namespace QwenBotQ.SDK.Events;


public class MessageContext
{
    public long SelfId { get; set; }
    public long MessageId { get; set; }
    public long UserId { get; set; }
    public string RawMessage { get; set; } = string.Empty;
    public List<MessageSegment> Message { get; set; } = new();
    public DateTime Time { get; set; }
    
    /// <summary>
    /// 提取纯文本内容
    /// </summary>
    public string GetPlainText()
    {
        return string.Join("", Message
            .Where(m => m.Type == "text")
            .Select(m => m.Data?.GetValueOrDefault("text")?.ToString() ?? ""));
    }
    
    /// <summary>
    /// 获取@的用户ID列表
    /// </summary>
    public List<long> GetMentionedUsers()
    {
        return Message
            .Where(m => m.Type == "at")
            .Select(m => 
            {
                var idStr = m.Data?.GetValueOrDefault("qq")?.ToString();
                return long.TryParse(idStr, out var id) ? id : 0;
            })
            .Where(id => id > 0)
            .ToList();
    }
    
    /// <summary>
    /// 回复消息
    /// </summary>
    public Func<string, bool, Task>? ReplyAsync { get; set; }
    
    /// <summary>
    /// 回复消息（复杂消息）
    /// </summary>
    public Func<List<object>, bool, Task>? ReplyMessageAsync { get; set; }
}

public class GroupMessageContext : MessageContext
{
    public long GroupId { get; set; }
}