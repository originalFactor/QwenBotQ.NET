using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QwenBotQ.SDK.Commands;
using QwenBotQ.SDK.Context;
using QwenBotQ.SDK.OneBotS;
using QwenBotQ.SDK.DatabaseS;
using QwenBotQ.SDK.Models.OneBot.Events;

namespace QwenBotQ.SDK.Core;

public class Bot
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Bot> _logger;
    public CommandManager CommandManagerService { get; init; }
    public OneBot OneBotService { get; init; }
    public DatabaseS.Database DataBaseService { get; init; }
    
    public Bot(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<Bot>>();
        CommandManagerService = serviceProvider.GetRequiredService<CommandManager>();
        OneBotService = serviceProvider.GetRequiredService<OneBot>();
        DataBaseService = serviceProvider.GetRequiredService<Database>();
        
        // 注册内部事件处理
        OneBotService.RegisterEventHandler<MessageEvent>(HandleInternalMessageAsync);
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
    
    private async Task HandleInternalMessageAsync(MessageEvent context)
    {
        try
        {
            var messageContext = context switch
            {
                GroupMessageEvent g => new GroupMessageContext
                {
                    Bot = OneBotService,
                    Event = g
                },
                _ => new MessageContext
                {
                    Bot = OneBotService,
                    Event = context
                }
            };
            await CommandManagerService.HandleMessageAsync(messageContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling message event");
        }
    }
}