using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QwenBotQ.Commands;
using QwenBotQ.SDK.Core;
using QwenBotQ.SDK.Extensions;
using QwenBotQ.SDK.Services;
using System.Reflection;

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
        var botSettings = builder.Configuration.GetSection("BotSettings");

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
        builder.Services.AddOneBotService<OneBotService>();
        builder.Services.AddDatabaseService<DatabaseService>();

        // 添加后台服务
        builder.Services.AddHostedService<BotHostedService>();

        var host = builder.Build();

        await host.RunAsync();
    }
}

public class BotHostedService : BackgroundService
{
    private readonly IBotSDK _botSDK;
    private readonly ILogger<BotHostedService> _logger;
    
    public BotHostedService(IBotSDK botSDK, ILogger<BotHostedService> logger)
    {
        _botSDK = botSDK;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting QwenBot Demo...");
            
            // 自动发现并注册Commands程序集中的所有命令
            _botSDK.CommandManagerService.DiscoverCommands(Assembly.GetAssembly(typeof(UserInfoCommand))!);
            
            // 也可以手动注册特定命令
            // _botSDK.RegisterCommand<UserInfoCommand>();
            
            // 注册事件处理器（可选）
            _botSDK.OnMessage += (context) =>
            {
                _logger.LogInformation($"Received message from {context.UserId}: {context.GetPlainText()}");
                return Task.CompletedTask;
            };
            
            _botSDK.OnGroupMessage += (context) =>
            {
                _logger.LogInformation($"Received group message from {context.UserId} in group {context.GroupId}: {context.GetPlainText()}");
                return Task.CompletedTask;
            };
            
            // 启动Bot SDK
            await _botSDK.StartAsync();
            
            _logger.LogInformation("QwenBot Demo started successfully. Press Ctrl+C to stop.");
            
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
            _logger.LogInformation("QwenBot Demo stopped.");
        }
    }
}