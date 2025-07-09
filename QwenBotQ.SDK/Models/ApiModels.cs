namespace QwenBotQ.SDK.Models;

public class ApiResponse
{
    public string Status { get; set; } = "ok";
    public int Retcode { get; set; } = 0;
    public string? Message { get; set; }
    public object? Data { get; set; }
    public long Echo { get; set; }
}

public class MessageSegment
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object>? Data { get; set; }
}