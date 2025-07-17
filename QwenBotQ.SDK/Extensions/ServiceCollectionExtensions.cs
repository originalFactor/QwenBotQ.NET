using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QwenBotQ.SDK.Commands;
using QwenBotQ.SDK.Core;
using QwenBotQ.SDK.OneBotS;
using QwenBotQ.SDK.DatabaseS;

namespace QwenBotQ.SDK.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加QwenBot SDK服务
    /// </summary>
    public static IServiceCollection AddQwenBotSDK(this IServiceCollection services, 
        Action<BotSDKOptions>? configureOptions = null)
    {
        var options = new BotSDKOptions();
        configureOptions?.Invoke(options);
        
        // 注册配置
        services.AddSingleton(options);
        
        // 注册核心服务
        services.AddSingleton<CommandManager>();
        services.AddSingleton<Bot>();
        
        // 注册日志（如果未注册）
        if (!services.Any(x => x.ServiceType == typeof(ILoggerFactory)))
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole().SetMinimumLevel(LogLevel.Information);
            });
        }
        
        return services;
    }
    
    /// <summary>
    /// 添加OneBot服务实现
    /// </summary>
    public static IServiceCollection AddOneBotService<T>(this IServiceCollection services)
        where T : OneBot
    {
        services.AddSingleton<OneBot>(provider =>
        {
            var options = provider.GetRequiredService<BotSDKOptions>();
            var logger = provider.GetRequiredService<ILogger<T>>();
            
            // 使用反射创建实例，传递必要的参数
            if (typeof(T) == typeof(OneBotS.OneBot))
            {
                return new OneBotS.OneBot(options.OneBotServerUrl, options.OneBotToken,
                    provider.GetRequiredService<ILogger<OneBotS.OneBot>>()) as T ?? throw new InvalidOperationException();
            }
            
            // 对于其他实现，尝试使用默认构造函数
            return ActivatorUtilities.CreateInstance<T>(provider);
        });
        return services;
    }
    
    /// <summary>
    /// 添加数据库服务实现
    /// </summary>
    public static IServiceCollection AddDatabaseService<T>(this IServiceCollection services)
        where T : Database
    {
        services.AddSingleton<Database>(provider =>
        {
            var options = provider.GetRequiredService<BotSDKOptions>();
            var logger = provider.GetRequiredService<ILogger<T>>();
            
            // 使用反射创建实例，传递必要的参数
            if (typeof(T) == typeof(DatabaseS.Database))
            {
                return new DatabaseS.Database(options.MongoConnectionString, options.DatabaseName,
                    provider.GetRequiredService<ILogger<DatabaseS.Database>>()) as T ?? throw new InvalidOperationException();
            }
            
            // 对于其他实现，尝试使用默认构造函数
            return ActivatorUtilities.CreateInstance<T>(provider);
        });
        return services;
    }
}

public class BotSDKOptions
{
    public string OneBotServerUrl { get; set; } = "ws://localhost:8080";
    public string? OneBotToken { get; set; }
    public string MongoConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "QwenBot";
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}