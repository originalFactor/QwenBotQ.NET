using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QwenBotQ.SDK.Events;
using System.Reflection;

namespace QwenBotQ.SDK.Commands;

public class CommandManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandManager> _logger;
    private readonly List<Type> _commandTypes = new();
    private readonly List<ICommand> _commands = new();
    
    public CommandManager(IServiceProvider serviceProvider, ILogger<CommandManager> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }
    
    /// <summary>
    /// 注册命令类型
    /// </summary>
    public void RegisterCommand<T>() where T : class, ICommand
    {
        _commandTypes.Add(typeof(T));
        _logger.LogInformation($"Registered command type: {typeof(T).Name}");
    }
    
    /// <summary>
    /// 从程序集中自动发现并注册命令
    /// </summary>
    public void DiscoverCommands(Assembly assembly)
    {
        var commandTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(ICommand).IsAssignableFrom(t))
            .ToList();
            
        foreach (var type in commandTypes)
        {
            _commandTypes.Add(type);
            _logger.LogInformation($"Discovered command: {type.Name}");
        }
    }
    
    /// <summary>
    /// 初始化所有命令实例
    /// </summary>
    public void InitializeCommands()
    {
        foreach (var commandType in _commandTypes)
        {
            try
            {
                var command = (ICommand)ActivatorUtilities.CreateInstance(_serviceProvider, commandType);
                _commands.Add(command);
                _logger.LogInformation($"Initialized command: {command.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to initialize command: {commandType.Name}");
            }
        }
    }
    
    /// <summary>
    /// 处理消息事件
    /// </summary>
    public async Task HandleMessageAsync(MessageContext context)
    {
        foreach (var command in _commands)
        {
            try
            {
                if (command.CanHandle(context))
                {
                    _logger.LogInformation($"Executing command: {command.Name} for message: {context.GetPlainText()}");
                    await command.ExecuteAsync(context);
                    _logger.LogInformation($"Command executed: {command.Name}");
                    return; // 只执行第一个匹配的命令
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing command: {command.Name}");
            }
        }
    }
    
    /// <summary>
    /// 处理群消息事件
    /// </summary>
    public async Task HandleGroupMessageAsync(GroupMessageContext context)
    {
        bool commandExecuted = false;
        
        // 首先尝试群命令
        foreach (var command in _commands.OfType<IGroupCommand>())
        {
            try
            {
                if (command.CanHandle(context))
                {
                    _logger.LogInformation($"Executing group command: {command.Name} for message: {context.GetPlainText()}");
                    await command.ExecuteAsync(context);
                    commandExecuted = true;
                    return; // 只执行第一个匹配的命令
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing group command: {command.Name}");
                commandExecuted = true; // 即使出错也标记为已执行，避免重复
                return;
            }
        }
        
        // 如果没有群命令匹配，尝试普通命令（但排除已经作为群命令处理过的）
        if (!commandExecuted)
        {
            foreach (var command in _commands.Where(c => !(c is IGroupCommand)))
            {
                try
                {
                    if (command.CanHandle(context))
                    {
                        _logger.LogInformation($"Executing command: {command.Name} for message: {context.GetPlainText()}");
                        await command.ExecuteAsync(context);
                        return; // 只执行第一个匹配的命令
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error executing command: {command.Name}");
                    return;
                }
            }
        }
    }
    
    /// <summary>
    /// 获取所有已注册的命令
    /// </summary>
    public IEnumerable<ICommand> GetCommands()
    {
        return _commands;
    }
}