using QwenBotQ.SDK.Context;
using QwenBotQ.SDK.Extensions;
using System.Reflection;

namespace QwenBotQ.SDK.Commands;

public abstract class Command
{
    public virtual string Name { get; protected set; } = string.Empty;
    public virtual string Description { get; protected set; } = string.Empty;
    public virtual string[] Triggers { get; set; } = Array.Empty<string>();
    
    protected Command()
    {
        // 从特性中获取命令信息
        var commandAttr = GetType().GetCustomAttribute<CommandAttribute>();
        if (commandAttr != null)
        {
            Name = commandAttr.Name;
            Description = commandAttr.Description;
            Triggers = commandAttr.Triggers;
        }
    }
    
    public virtual bool CanHandle(MessageContext context)
    {
        var plainText = context.Event?.Message.GetPlainText().Trim();
        return Triggers.Any(trigger => plainText?.StartsWith(trigger, StringComparison.OrdinalIgnoreCase) == true);
    }
    
    public abstract Task ExecuteAsync(MessageContext context);
}

public abstract class GroupCommand : Command
{
    protected GroupCommand()
    {
        Description += " (群组专用)";
    }
    public override bool CanHandle(MessageContext context)
    {
        return context is GroupMessageContext && base.CanHandle(context);
    }

    public abstract Task ExecuteAsync(GroupMessageContext context);

    public override Task ExecuteAsync(MessageContext context)
    {
        if (context is GroupMessageContext groupContext)
        {
            return ExecuteAsync(groupContext);
        }
        return Task.CompletedTask;
    }
}