using Microsoft.Extensions.Logging;
using QwenBotQ.SDK.Commands;
using QwenBotQ.SDK.Context;
using QwenBotQ.SDK.Messages;
using System.Text;


namespace QwenBotQ.Commands;

[Command("帮助", "显示可用命令列表", "帮助", "help", "?")]
public class HelpCommand : Command
{
    private readonly ILogger<HelpCommand> _logger;
    private readonly CommandManager _commandManager;
    
    public HelpCommand(ILogger<HelpCommand> logger, CommandManager commandManager)
    {
        _logger = logger;
        _commandManager = commandManager;
    }

    public override async Task ExecuteAsync(MessageContext context)
    {
        try
        {
            var commands = _commandManager.GetCommands();
            var builder = new StringBuilder("主人可以这么指挥喵喵哦：\n\n");
            
            foreach (var command in commands)
            {
                builder.AppendLine($"""
                    {command.Name}: {command.Description}
                      触发词: {string.Join(", ", command.Triggers)}

                    """);
            }

            await context.Quick(new Message(builder.ToString().Trim()));

            _logger.LogInformation($"Displayed help for user {context.Event?.UserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Help command");

            await context.Quick(new Message("喵喵好像坏掉了，怎么办……"));
        }
    }
}