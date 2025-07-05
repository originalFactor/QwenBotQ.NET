using Microsoft.Extensions.Logging;
using QwenBotQ.NET.Models.OneBot;
using QwenBotQ.NET.Services.Interfaces;
using QwenBotQ.NET.Views;

namespace QwenBotQ.NET.Controllers
{
    public class MessageController
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<MessageController> _logger;

        public MessageController(IDatabaseService databaseService, ILogger<MessageController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        public async Task HandleMessageAsync(MessageEventModel msg)
        {
            string[] plainText = msg.ExtractPlainText().Trim().Split();
            if (plainText.Length == 0) return;

            // _logger.LogInformation($"Received message: {string.Join(" ", plainText)}");

            if (plainText[0] == "用户信息")
            {
                await HandleGetInfoAsync(msg);
            }
        }

        private async Task HandleGetInfoAsync(MessageEventModel msg)
        {
            var mentioned = msg.GetMentioned();
            var user = await _databaseService.GetUserAsync(mentioned.Length > 0 ? mentioned[0] : msg.UserId.ToString());
            
            // 使用视图模型格式化用户信息
            string userInfoText = UserInfoView.FormatUserInfo(user);
            await msg.ReplyAsync(userInfoText);
        }
    }
}