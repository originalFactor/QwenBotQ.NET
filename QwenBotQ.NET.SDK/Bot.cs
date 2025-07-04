using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using QwenBotQ.NET.SDK.Models;

namespace QwenBotQ.NET.SDK;

public class Bot
{
    private readonly ClientWebSocket _client = new();
    private readonly CancellationTokenSource _cancellation = new();

    public event Func<PrivateMessageEvent, Task>? OnPrivateMessage;
    public event Func<GroupMessageEvent, Task>? OnGroupMessage;

    public async Task ConnectAsync(Uri uri, string? token = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(token))
            {
                _client.Options.SetRequestHeader("Authorization", $"Bearer {token}");
            }
            await _client.ConnectAsync(uri, CancellationToken.None);
            _ = Task.Run(ReceiveLoop);
        }
        catch (WebSocketException ex)
        {
            throw new Exception($"无法连接到 OneBot 服务器（{uri}），请确保服务器已启动：{ex.Message}", ex);
        }
    }

    private async Task ReceiveLoop()
    {
        var buffer = new ArraySegment<byte>(new byte[2048]);
        while (_client.State == WebSocketState.Open)
        {
            var result = await _client.ReceiveAsync(buffer, _cancellation.Token);
            if (buffer.Array == null) continue;
            var json = Encoding.UTF8.GetString(buffer.Array, 0, result.Count);
            // Console.WriteLine($"接收到消息：{json}");
            var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            if (root.TryGetProperty("post_type", out var postTypeElement))
            {
                var postType = postTypeElement.GetString();
                if (postType == "message")
                {
                    if (root.TryGetProperty("message_type", out var messageTypeElement))
                    {
                        var messageType = messageTypeElement.GetString();
                        if (messageType == "private")
                        {
                            var privateMessage = JsonSerializer.Deserialize<PrivateMessageEvent>(json);
                            if (OnPrivateMessage != null && privateMessage != null)
                                await OnPrivateMessage.Invoke(privateMessage);
                        }
                        else if (messageType == "group")
                        {
                            var groupMessage = JsonSerializer.Deserialize<GroupMessageEvent>(json);
                            if (OnGroupMessage != null && groupMessage != null)
                                await OnGroupMessage.Invoke(groupMessage);
                        }
                    }
                }
            }
        }
    }

    public async Task DisconnectAsync()
    {
        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client requested disconnect", CancellationToken.None);
        _cancellation.Cancel();
    }
}