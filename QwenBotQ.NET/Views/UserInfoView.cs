using QwenBotQ.NET.Models;

namespace QwenBotQ.NET.Views;

public class UserInfoView
{
    public static string FormatUserInfo(UserModel? user)
    {
        return $"""
        用户信息：
        昵称: {user?.Nick ?? "未知"}
        QQ号: {user?.Id ?? "未知"}
        权限: {user?.Permission ?? 0}
        系统提示: {user?.SystemPrompt[..Math.Min(user?.SystemPrompt?.Length ?? 0, 15)] ?? "未知"}
        温度: {user?.Temprature ?? 1.0}
        频率惩罚: {user?.FrequencyPenalty ?? 0.0}
        重复惩罚: {user?.PresencePenalty ?? 0.0}
        硬币: {user?.Coins ?? 0}
        签到过期: {user?.SignExpire.ToString() ?? "未知"}
        模型: {user?.model ?? "未知"}
        个人资料过期: {user?.ProfileExpire.ToString() ?? "未知"}
        绑定权重: {Math.Round(user?.BindPower ?? 0.0, 2)}
        绑定QQ号: {user?.Binded?.Ident ?? "无"}
        绑定过期: {user?.Binded?.Expire.ToString() ?? "无"}
        """;
    }
}