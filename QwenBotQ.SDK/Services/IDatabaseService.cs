using QwenBotQ.SDK.Models;

namespace QwenBotQ.SDK.Services;

public interface IDatabaseService
{
    /// <summary>
    /// 获取用户信息
    /// </summary>
    Task<UserModel?> GetUserAsync(string userId);
    
    /// <summary>
    /// 保存用户信息
    /// </summary>
    Task SaveUserAsync(UserModel user);

}