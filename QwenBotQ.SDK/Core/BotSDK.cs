using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QwenBotQ.SDK.Commands;
using QwenBotQ.SDK.Events;
using QwenBotQ.SDK.Models;
using QwenBotQ.SDK.Services;
using System.Reflection;

namespace QwenBotQ.SDK.Core;

public class BotSDK : IBotSDK
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BotSDK> _logger;
    public CommandManager CommandManagerService { get; init; }
    public IOneBotService OneBotService { get; init; }
    public IDatabaseService DataBaseService { get; init; }

    public event Func<MessageContext, Task>? OnMessage;
    public event Func<GroupMessageContext, Task>? OnGroupMessage;
    
    public BotSDK(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<BotSDK>>();
        CommandManagerService = serviceProvider.GetRequiredService<CommandManager>();
        OneBotService = serviceProvider.GetRequiredService<IOneBotService>();
        DataBaseService = serviceProvider.GetRequiredService<IDatabaseService>();
        
        // 注册内部事件处理
        OneBotService.OnMessage += HandleInternalMessageAsync;
        OneBotService.OnGroupMessage += HandleInternalGroupMessageAsync;
    }
    
    public async Task StartAsync()
    {
        _logger.LogInformation("Starting Bot SDK...");
        
        // 初始化命令
        CommandManagerService.InitializeCommands();
        
        // 启动OneBot服务
        await OneBotService.StartAsync();
        
        _logger.LogInformation("Bot SDK started successfully.");
    }
    
    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping Bot SDK...");
        await OneBotService.StopAsync();
        _logger.LogInformation("Bot SDK stopped.");
    }
    
    private async Task HandleInternalMessageAsync(MessageContext context)
    {
        try
        {
            // 先执行命令处理
            await CommandManagerService.HandleMessageAsync(context);
            
            // 然后触发外部事件
            if (OnMessage != null)
            {
                await OnMessage(context);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling message event");
        }
    }
    private async Task HandleInternalGroupMessageAsync(GroupMessageContext context)
    {
        try
        {
            // 先执行命令处理
            await CommandManagerService.HandleGroupMessageAsync(context);
            
            // 然后触发外部事件
            if (OnGroupMessage != null)
            {
                await OnGroupMessage(context);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling group message event");
        }
    }
}