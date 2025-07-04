using Microsoft.Extensions.Logging;
using QwenBotQ.NET.OneBot.Models;
using Core = QwenBotQ.NET.OneBot.Core;

namespace QwenBotQ.NET
{
    class Program
    {
        static readonly ManualResetEvent ExitEvent = new ManualResetEvent(false);
        static readonly ILoggerFactory LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(
            builder => builder.AddConsole()
        );
        static readonly ILogger Logger = LoggerFactory.CreateLogger<Program>();
        static  Core.OneBot ?_oneBot;

        static async Task Main(string[] args)
        {
            _oneBot = new Core.OneBot(
                uri: args.Length > 0 ? args[0] : "ws://127.0.0.1:3001/ws",
                // ReSharper disable once StringLiteralTypo
                token: args.Length > 1 ? args[1] : "napcatqq",
                loggerFactory: LoggerFactory
            );

            _oneBot.OnEvent += async (eventModel) =>
            {
                if (eventModel is GroupMessageEventModel groupMsg)
                {
                    if (groupMsg.RawMessage.EndsWith("getCQ"))
                        await groupMsg.ReplyAsync(groupMsg.RawMessage);
                }
                else if (eventModel is MessageEventModel msg)
                {
                    if (msg.RawMessage.EndsWith("getCQ"))
                        await msg.ReplyAsync(msg.RawMessage);
                }
            };

            await _oneBot.ConnectAsync();

            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true; // Prevent the process from terminating immediately
                ExitEvent.Set(); // Signal the exit event
            };

            ExitEvent.WaitOne(); // Wait for the exit event to be set

            await _oneBot.CloseAsync();

            Logger.LogInformation("Exiting application...");
        }
    }
}

