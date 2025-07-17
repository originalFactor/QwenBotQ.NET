using Microsoft.Extensions.Logging;
using QwenBotQ.SDK.Commands;
using QwenBotQ.SDK.Context;
using QwenBotQ.SDK.DatabaseS;
using QwenBotQ.SDK.OneBotS;

namespace QwenBotQ.Commands;

[Command("签到", "每日签到获取积分", "签到", "sign")]
public class SignCommand : Command
{
    readonly ILogger<SignCommand> logger;
    readonly Database database;
    readonly Random random = new();

    public SignCommand(OneBot ob, Database db, ILogger<SignCommand> l)
    {
        logger = l;
        database = db;
    }

    public override async Task ExecuteAsync(MessageContext context)
    {
        try
        {
            var user = await database.GetUserOrCreateAsync(
                context.Event!.UserId.ToString(),
                context.Event.Sender.Nickname ?? await context.GetUserNickAsync(context.Event.UserId, false) ?? "未知用户"
            );

            if (user.SignExpire > DateTime.Now)
            {
                await context.Quick(
                    $"主人，今天已经签到过了喵！\n" +
                    $"下次签到时间：{user.SignExpire:yyyy/MM/dd}"
                );
                return;
            }

            var change = random.Next(10, 99);
            user.Coins += change;
            var expire = DateTime.Now.AddDays(1);
            user.SignExpire = expire;

            await database.SaveUserAsync(user);

            await context.Quick(
                $"""
                喵喵帮主人签到啦！
                本次获得 {change} 积分！
                下次签到时间：{expire:yyyy/MM/dd}
                """
            );
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Exception occured during handling sign.");
            await context.Quick("什么地方不对劲啊喵……");
            return;
        }
    }
}
