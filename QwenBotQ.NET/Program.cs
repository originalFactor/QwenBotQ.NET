using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QwenBotQ.NET.Controllers;
using QwenBotQ.NET.Services;
using QwenBotQ.NET.Services.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace QwenBotQ.NET
{
    internal class Program
    {
        static ManualResetEvent exitEvent = new(false);
        static IServiceProvider? serviceProvider;

        static async Task Main(string[] args)
        {
            // 配置依赖注入
            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();

            // 获取OneBotService
            var oneBotService = serviceProvider.GetRequiredService<IOneBotService>();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // 防止进程立即终止
                exitEvent.Set(); // 触发退出事件
            };

            try
            {
                await oneBotService.ConnectAsync();
                logger.LogInformation("Bot connected. Press Ctrl+C to exit.");
                exitEvent.WaitOne(); // 等待退出事件被触发
            }
            catch (Exception ex)
            {
                logger.LogError($"An error occurred: {ex.Message}");
            }
            finally
            {
                await oneBotService.CloseAsync();
                logger.LogInformation("Bot disconnected.");
            }
        }

        static void ConfigureServices(ServiceCollection services)
        {
            // 配置日志
            services.AddLogging(builder =>
            {
                builder
                    .AddSimpleConsole(options =>
                    {
                        options.IncludeScopes = true;
                        options.TimestampFormat = "G";
                    })
                    .SetMinimumLevel(LogLevel.Information);
            });

            // 注册服务
            services.AddSingleton<IConfigService, ConfigService>();
            services.AddSingleton<IDatabaseService>(provider =>
            {
                var configService = provider.GetRequiredService<IConfigService>();
                return new DatabaseService(configService.MongoUri, configService.MongoDbName);
            });

            // 注册控制器
            services.AddSingleton<MessageController>();

            // 注册WebSocketService
            services.AddSingleton<IWebSocketService>(provider =>
            {
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                return new WebSocketService(loggerFactory);
            });

            // 注册OneBotService
            services.AddSingleton<IOneBotService>(provider =>
            {
                var configService = provider.GetRequiredService<IConfigService>();
                var webSocketService = provider.GetRequiredService<IWebSocketService>();
                var loggerFactory = provider.GetRequiredService<ILoggerFactory>();
                var oneBotService = new OneBotService(
                    configService.OneBotServer,
                    configService.OneBotToken,
                    webSocketService,
                    loggerFactory
                );

                // 注册消息处理器
                var messageController = provider.GetRequiredService<MessageController>();
                oneBotService.AddMessageHandler(messageController.HandleMessageAsync);

                return oneBotService;
            });
        }
    }
}






