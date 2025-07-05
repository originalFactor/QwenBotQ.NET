using QwenBotQ.NET.Models;

namespace QwenBotQ.NET.Services.Interfaces;

public interface IDatabaseService
{
    Task<UserModel?> GetUserAsync(string userId);
}