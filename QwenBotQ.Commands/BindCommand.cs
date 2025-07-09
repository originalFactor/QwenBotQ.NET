using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using QwenBotQ.SDK.Commands;
using QwenBotQ.SDK.Core;
using QwenBotQ.SDK.Events;
using QwenBotQ.SDK.Models;
using Newtonsoft.Json;

namespace QwenBotQ.Commands
{
    [Command("今日XX", "绑定今日XX", "今日")]
    internal class BindCommand : BaseGroupCommand
    {
        static readonly Random _random = new();
        readonly IBotSDK _botSDK;
        readonly ILogger<BindCommand> _logger;
        readonly CommandManager _commandManager;

        public BindCommand(IBotSDK botSDK, ILogger<BindCommand> logger, CommandManager commandManager)
        {
            _botSDK = botSDK;
            _logger = logger;
            _commandManager = commandManager;
        }

        public override async Task ExecuteAsync(GroupMessageContext context)
        {
            var type = context.GetPlainText().Trim()[2..] ?? "老公";
            try
            {
                var user = await Utility.GetGroupMemberAsync(_botSDK, context.GroupId, context.UserId);
                var userModel = await Utility.GetUserOrCreateAsync(user!, _botSDK, _logger);
                if(userModel.Binded != null && userModel.Binded.Expire > DateTime.Now)
                {
                    var wifeMem = await Utility.GetGroupMemberAsync(_botSDK, context.GroupId, long.Parse(userModel.Binded.Ident));
                    if (wifeMem != null)
                    {
                        var response = $"""
                            [CQ:at,qq={context.UserId}]
                            你已绑定今日{type}为：
                            {(string.IsNullOrEmpty(wifeMem.Card) ? wifeMem.Nickname : wifeMem.Card)} ({wifeMem.UserId})
                            [CQ:image,file=http://q.qlogo.cn/headimg_dl?dst_uin={wifeMem.UserId}&spec=640&img_type=jpg]
                            绑定有效期至：{userModel.Binded.Expire:yyyy-MM-dd}
                            """;
                        await _botSDK.OneBotService.SendCQMessageAsync(response, null, context.GroupId);
                        return;
                    }
                    if (context.ReplyAsync != null)
                    {
                        await context.ReplyAsync($"你的{type}好像不在这里……", false);
                    }
                    return;
                }
                var randomMember = await GetRandomMemberAsync(context);
                if(randomMember != null)
                {
                    var expire = await Utility.BindUserAsync(_botSDK, _logger, user!, randomMember.Item2);
                    var response = $"""
                    [CQ:at,qq={context.UserId}]
                    绑定成功！🎉
                    你已成功绑定今日{type}为：
                    {randomMember.Item1.Nick} ({randomMember.Item1.Id})
                    [CQ:image,file=http://q.qlogo.cn/headimg_dl?dst_uin={randomMember.Item1.Id}&spec=640&img_type=jpg]
                    绑定有效期至：{expire:yyyy-MM-dd}
                    """;
                    await _botSDK.OneBotService.SendCQMessageAsync(response, null, context.GroupId);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Bind command");
                
            }
            if (context.ReplyAsync != null)
            {
                await context.ReplyAsync($"你的{type}好像跑掉了……", false);
            }
        }

        async Task<Tuple<UserModel, GroupMemberData>?> GetRandomMemberAsync(GroupMessageContext context)
        {
            var members = await Utility.GetGroupMemberListAsync(_botSDK, context.GroupId);
            var availableMembers = members
                ?.Where(m => m.UserId != context.SelfId && m.UserId != context.UserId)
                .ToList();
            if (availableMembers?.Count == 0)
            {
                _logger.LogWarning($"No available members found in group {context.GroupId} for random selection.");
                return null;
            }

            int i = 0;
            UserModel user;
            GroupMemberData randomMember;
            do
            {
                randomMember = availableMembers![_random.Next(availableMembers.Count)]!;
                user = await Utility.GetUserOrCreateAsync(randomMember, _botSDK, _logger);
            } while (i < 3 && (user.Binded != null && user.Binded.Expire > DateTime.Now));

            return i<3 ? new Tuple<UserModel, GroupMemberData>(user, randomMember) : null;
        }
    }

}
