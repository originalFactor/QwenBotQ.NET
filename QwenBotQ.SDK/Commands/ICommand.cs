using QwenBotQ.SDK.Events;

namespace QwenBotQ.SDK.Commands;

public interface ICommand
{
    /// <summary>
    /// 命令名称
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// 命令描述
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// 是否匹配该命令
    /// </summary>
    bool CanHandle(MessageContext context);
    
    /// <summary>
    /// 执行命令
    /// </summary>
    Task ExecuteAsync(MessageContext context);
}

public interface IGroupCommand : ICommand
{
    /// <summary>
    /// 是否匹配该群命令
    /// </summary>
    bool CanHandle(GroupMessageContext context);
    
    /// <summary>
    /// 执行群命令
    /// </summary>
    Task ExecuteAsync(GroupMessageContext context);
}