using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QwenBotQ.Commands;
using QwenBotQ.SDK.Core;
using QwenBotQ.SDK.Extensions;
using QwenBotQ.SDK.OneBotS;
using QwenBotQ.SDK.DatabaseS;
using System.Reflection;
using QwenBotQ.SDK.Models.OneBot.Events;
using Microsoft.Extensions.Configuration;

namespace QwenBotQ.Next;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // 配置日志
        builder.Services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        // 从配置文件读取设置
        //builder.Configuration.Sources.Clear();
        //builder.Configuration.SetBasePath(AppContext.BaseDirectory);
        //builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        var botSettings = builder.Configuration.GetSection("BotSettings");

        //Console.WriteLine($"OneBotServerUrl: {botSettings["OneBotServerUrl"]}");
        //Console.WriteLine($"OneBotToken: {botSettings["OneBotToken"]}");
        //Console.WriteLine($"MongoConnectionString: {botSettings["MongoConnectionString"]}");
        //Console.WriteLine($"DatabaseName: {botSettings["DatabaseName"]}");

        // 添加QwenBot SDK
        builder.Services.AddQwenBotSDK(options =>
        {
            // 确保从配置文件读取的值能正确设置
            var oneBotUrl = botSettings["OneBotServerUrl"];
            var oneBotToken = botSettings["OneBotToken"];
            var mongoConnection = botSettings["MongoConnectionString"];
            var dbName = botSettings["DatabaseName"];
            
            if (!string.IsNullOrEmpty(oneBotUrl))
                options.OneBotServerUrl = oneBotUrl;
            if (!string.IsNullOrEmpty(oneBotToken))
                options.OneBotToken = oneBotToken;
            if (!string.IsNullOrEmpty(mongoConnection))
                options.MongoConnectionString = mongoConnection;
            if (!string.IsNullOrEmpty(dbName))
                options.DatabaseName = dbName;
        });

        // 注册OneBot和Database服务的具体实现
        builder.Services.AddOneBotService<OneBot>();
        builder.Services.AddDatabaseService<Database>();

        // 添加后台服务
        builder.Services.AddHostedService<BotHostedService>();

        var host = builder.Build();

        await host.RunAsync();
    }
}

public class BotHostedService : BackgroundService
{
    private readonly Bot _botSDK;
    private readonly ILogger<BotHostedService> _logger;
    
    public BotHostedService(Bot botSDK, ILogger<BotHostedService> logger)
    {
        _botSDK = botSDK;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting QwenBotQ.Next...");
            
            // 自动发现并注册Commands程序集中的所有命令
            _botSDK.CommandManagerService.DiscoverCommands(Assembly.GetAssembly(typeof(UserInfoCommand))!);
            
            // 也可以手动注册特定命令
            // _botSDK.RegisterCommand<UserInfoCommand>();
            
            // 注册事件处理器（可选）
            _botSDK.OneBotService.RegisterEventHandler<MessageEvent>((context) =>
            {
                var g = context is GroupMessageEvent e ? e.GroupId.ToString() : "private";
                _logger.LogInformation($"[{g}] {context.Sender.Nickname}({context.UserId}): {context.RawMessage}");
                return Task.CompletedTask;
            });
            
            // 启动Bot SDK
            await _botSDK.StartAsync();
            
            _logger.LogInformation("QwenBotQ.Next started successfully. Press Ctrl+C to stop.");
            
            // 等待取消信号
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Bot service is stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while running the bot service");
        }
        finally
        {
            await _botSDK.StopAsync();
            _logger.LogInformation("QwenBotQ.NET stopped.");
        }
    }
}