using Microsoft.Extensions.Logging;
using QwenBotQ.SDK.Commands;
using QwenBotQ.SDK.Core;
using QwenBotQ.SDK.Events;


namespace QwenBotQ.Commands;

[Command("帮助", "显示可用命令列表", "帮助", "help", "?")]
public class HelpCommand : BaseCommand
{
    private readonly IBotSDK _botSDK;
    private readonly ILogger<HelpCommand> _logger;
    private readonly CommandManager _commandManager;
    
    public HelpCommand(IBotSDK botSDK, ILogger<HelpCommand> logger, CommandManager commandManager)
    {
        _botSDK = botSDK;
        _logger = logger;
        _commandManager = commandManager;
    }

    public override async Task ExecuteAsync(MessageContext context)
    {
        if (context is GroupMessageContext groupContext)
        {
            await ExecuteAsync(groupContext);
            return;
        }
        
        try
        {
            var commands = _commandManager.GetCommands();
            var helpText = "📋 可用命令列表：\n\n";
            
            foreach (var command in commands)
            {
                helpText += $"🔹 {command.Name}: {command.Description}\n";
            }
            
            helpText += "\n💡 使用方法: 直接发送命令名称即可";
            
            if (context.ReplyAsync != null)
            {
                await context.ReplyAsync(helpText, false);
            }
            
            _logger.LogInformation($"Displayed help for user {context.UserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Help command");
            
            if (context.ReplyAsync != null)
            {
                await context.ReplyAsync("获取帮助信息时发生错误，请稍后重试。", false);
            }
        }
    }
}