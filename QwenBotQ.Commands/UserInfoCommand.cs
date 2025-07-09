using Microsoft.Extensions.Logging;
using QwenBotQ.SDK.Commands;
using QwenBotQ.SDK.Core;
using QwenBotQ.SDK.Events;
using QwenBotQ.SDK.Models;

namespace QwenBotQ.Commands;

[Command("用户信息", "获取用户的详细信息", "用户信息", "userinfo")]
public class UserInfoCommand : BaseCommand
{
    private readonly IBotSDK _botSDK;
    private readonly ILogger<UserInfoCommand> _logger;
    
    public UserInfoCommand(IBotSDK botSDK, ILogger<UserInfoCommand> logger)
    {
        _botSDK = botSDK;
        _logger = logger;
    }
    
    public override async Task ExecuteAsync(MessageContext context)
    {
        try
        {
            // 获取@的用户或当前用户
            var mentionedUsers = context.GetMentionedUsers();
            var targetUserId = mentionedUsers.FirstOrDefault();
            
            if (targetUserId == 0)
            {
                targetUserId = context.UserId;
            }
            
            // 从数据库获取用户信息
            var user = await _botSDK.DataBaseService.GetUserAsync(targetUserId.ToString());
            
            // 格式化用户信息
            string userInfoText = FormatUserInfo(user);
            
            // 回复消息
            if (context.ReplyAsync != null)
            {
                await context.ReplyAsync(userInfoText, true);
            }
            
            _logger.LogInformation($"Displayed user info for user {targetUserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing UserInfo command");
            
            if (context.ReplyAsync != null)
            {
                await context.ReplyAsync("获取用户信息时发生错误，请稍后重试。", true);
            }
        }
    }
    
    private static string FormatUserInfo(UserModel? user)
    {
        if (user == null)
        {
            return "用户信息不存在，可能是新用户。";
        }
        
        return $"""
        用户信息：
        昵称: {user.Nick ?? "未知"}
        QQ号: {user.Id ?? "未知"}
        权限: {user.Permission}
        系统提示: {user.SystemPrompt?[..Math.Min(user.SystemPrompt?.Length ?? 0, 15)] ?? "未知"}
        温度: {user.Temprature}
        频率惩罚: {user.FrequencyPenalty}
        重复惩罚: {user.PresencePenalty}
        硬币: {user.Coins}
        签到过期: {user.SignExpire.ToLongDateString()}
        模型: {user.model ?? "未知"}
        个人资料过期: {user.ProfileExpire.ToLongDateString()}
        绑定权重: {Math.Round(user.BindPower, 2)}
        绑定QQ号: {user.Binded?.Ident ?? "无"}
        绑定过期: {user.Binded?.Expire.ToLongDateString() ?? "无"}
        """;
    }
}