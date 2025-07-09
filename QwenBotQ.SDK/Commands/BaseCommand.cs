using QwenBotQ.SDK.Events;
using System.Reflection;

namespace QwenBotQ.SDK.Commands;

public abstract class BaseCommand : ICommand
{
    public virtual string Name { get; protected set; } = string.Empty;
    public virtual string Description { get; protected set; } = string.Empty;
    protected virtual string[] Triggers { get; set; } = Array.Empty<string>();
    
    protected BaseCommand()
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
        var plainText = context.GetPlainText().Trim();
        return Triggers.Any(trigger => plainText.StartsWith(trigger, StringComparison.OrdinalIgnoreCase));
    }
    
    public abstract Task ExecuteAsync(MessageContext context);
}

public abstract class BaseGroupCommand : BaseCommand, IGroupCommand
{
    public virtual bool CanHandle(GroupMessageContext context)
    {
        return base.CanHandle(context);
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