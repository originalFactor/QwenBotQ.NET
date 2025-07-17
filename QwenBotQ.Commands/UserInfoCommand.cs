using Microsoft.Extensions.Logging;
using QwenBotQ.SDK.Commands;
using QwenBotQ.SDK.Core;
using QwenBotQ.SDK.Context;
using QwenBotQ.SDK.Models.Database;
using QwenBotQ.SDK.Messages;
using QwenBotQ.SDK.Extensions;

namespace QwenBotQ.Commands;

[Command("用户信息", "获取用户的详细信息", "用户信息", "userinfo")]
public class UserInfoCommand : Command
{
    private readonly Bot _botSDK;
    private readonly ILogger<UserInfoCommand> _logger;
    
    public UserInfoCommand(Bot botSDK, ILogger<UserInfoCommand> logger)
    {
        _botSDK = botSDK;
        _logger = logger;
    }
    
    public override async Task ExecuteAsync(MessageContext context)
    {
        var group = context is GroupMessageContext c ? c : null;
        try
        {
            // 获取@的用户或当前用户
            var mentionedUsers =  group?.Event?.Message.GetMentionedArray();
            var targetUserId = Convert.ToInt64(mentionedUsers?.FirstOrDefault());
            
            if (targetUserId == 0)
            {
                targetUserId = context.Event?.UserId ?? 0;
            }
            
            // 从数据库获取用户信息
            var user = await _botSDK.DataBaseService.GetUserAsync(targetUserId.ToString());
            
            // 格式化用户信息
            string userInfoText = FormatUserInfo(user);
            
            // 回复消息
            await context.Quick(new Message(userInfoText));
            
            _logger.LogInformation($"Displayed user info for user {targetUserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing UserInfo command");
            await context.Quick(new Message("获取用户信息时发生错误，请稍后重试。"));
        }
    }
    
    private static string FormatUserInfo(UserModel? user)
    {
        if (user == null)
        {
            return "喵喵还找不到你诶，发送签到注册一下吧~";
        }
        
        return $"""
        喵喵还记得主人的信息呢~

        ——基本信息——
        昵称: {user.Nick}
        QQ号: {user.Id}
        权限等级: {user.Permission}
        积分: {user.Coins}
        本日已签: {(user.SignExpire > DateTime.Now ? "✅" : "❌")}

        ——AI相关——
        模型: {user.model ?? "未知"}
        系统提示词: {user.SystemPrompt[..Math.Min(user.SystemPrompt.Length, 20)]}
        温度: {user.Temprature}
        频率惩罚: {user.FrequencyPenalty}
        重复惩罚: {user.PresencePenalty}
        
        ——关系系统——
        稀有度: {Math.Round(user.BindPower*100, 2)}%
        """;
    }
}