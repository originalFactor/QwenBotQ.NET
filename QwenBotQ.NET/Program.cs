using QwenBotQ.NET.OneBot.Core;
using QwenBotQ.NET.OneBot.Models;
using Microsoft.Extensions.Logging;

class Program
{
    static OneBot ?bot;
    static ManualResetEvent exitEvent = new(false);
    static ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
    {
        builder
            .AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "HH:mm:ss ";
            })
            .SetMinimumLevel(LogLevel.Information);
    });
    static ILogger logger = loggerFactory.CreateLogger<Program>();
    static async Task Main(string[] args)
    {
        bot = new OneBot(
            args.Length > 0 ? args[0] : "ws://127.0.0.1:3001/ws",
            args.Length > 1 ? args[1] : "napcatqq",
            loggerFactory
        );

        bot.AddCallback<GroupMessageEventModel>(async (groupMsg) =>
        {
            bool isToMe = groupMsg.IsToMe();
            logger.LogInformation($"IsToMe: {isToMe}");
            if (!isToMe) return;
            await bot.GetGroupMemberInfoAsync(
                groupMsg.GroupId,
                groupMsg.UserId,
                async (resp) =>
                {
                    await groupMsg.ReplyAsync(resp.Data.Nickname);
                }
            );
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
    }
}
