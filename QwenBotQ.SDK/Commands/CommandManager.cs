using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QwenBotQ.SDK.Context;
using System.Reflection;

namespace QwenBotQ.SDK.Commands;

public class CommandManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CommandManager> _logger;
    private readonly List<Type> _commandTypes = new();
    private readonly List<Command> _commands = new();
    
    public CommandManager(IServiceProvider serviceProvider, ILogger<CommandManager> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// 注册命令类型
    /// </summary>
    public void RegisterCommand<T>() where T : Command
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
            .Where(t => t.IsClass && !t.IsAbstract && typeof(Command).IsAssignableFrom(t))
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
                var command = (Command)ActivatorUtilities.CreateInstance(_serviceProvider, commandType);
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
                    _logger.LogInformation($"Executing command: {command.Name} for message: {context.Event?.RawMessage}");
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
    /// 获取所有已注册的命令
    /// </summary>
    public IEnumerable<Command> GetCommands()
    {
        return _commands;
    }
}