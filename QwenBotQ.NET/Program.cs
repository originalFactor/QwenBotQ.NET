using QwenBotQ.NET.OneBot.Core;
using QwenBotQ.NET.OneBot.Models;
using Microsoft.Extensions.Logging;
using QwenBotQ.NET;

var exitEvent = new ManualResetEvent(false);
var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.TimestampFormat = "G";
        })
        .SetMinimumLevel(LogLevel.Information);
});
var logger = loggerFactory.CreateLogger<Program>();
var config = new ConfigModel();
var bot = new OneBot(
    config.OneBotServer,
    config.OneBotToken,
    loggerFactory
);
var db = new DataBase(config.MongoUri, config.MongoDbName);

bot.AddCallback<MessageEventModel>(async (msg) =>
{
    if (!msg.IsToMe()) return;
    var user = await db.GetUserAsync(msg.UserId.ToString());
    string r = $"""
    Name: {user?.Nick ?? "Unknown"}
    ID: {user?.Id ?? "Unknown"}
    Permission: {user?.Permission ?? 0}
    System Prompt: {user?.SystemPrompt[..Math.Min(user?.SystemPrompt?.Length ?? 0, 15)] ?? "Unknown"}
    Temperature: {user?.Temprature ?? 1.0}
    Frequency Penalty: {user?.FrequencyPenalty ?? 0.0}
    Presence Penalty: {user?.PresencePenalty ?? 0.0}
    Coins: {user?.Coins ?? 0}
    Sign Expire: {user?.SignExpire.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown"}
    Model: {user?.model ?? "Unknown"}
    Profile Expire: {user?.ProfileExpire.ToString("yyyy-MM-dd HH:mm:ss") ?? "Unknown"}
    Bind Power: {user?.BindPower ?? 0.0}
    Binded ID: {user?.Binded?.Ident ?? "None"}
    Binded Expire: {user?.Binded?.Expire.ToString("yyyy-MM-dd HH:mm:ss") ?? "None"}
    """;
    await msg.ReplyAsync(r);
});

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true; // Prevent the process from terminating immediately
    exitEvent.Set(); // Signal the exit event
};

try
{
    await bot.ConnectAsync();
    logger.LogInformation("Bot connected. Press Ctrl+C to exit.");
    exitEvent.WaitOne(); // Wait for the exit event to be set
}
catch (Exception ex)
{
    logger.LogError($"An error occurred: {ex.Message}");
}
finally
{
    await bot.CloseAsync();
    logger.LogInformation("Bot disconnected.");
}
