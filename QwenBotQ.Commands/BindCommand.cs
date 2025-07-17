using Microsoft.Extensions.Logging;
using QwenBotQ.SDK.Commands;
using QwenBotQ.SDK.Context;
using QwenBotQ.SDK.Models.Database;
using QwenBotQ.SDK.Messages;
using QwenBotQ.SDK.Models.OneBot.API;
using QwenBotQ.SDK.Extensions;
using QwenBotQ.SDK.DatabaseS;
using System.Diagnostics;

namespace QwenBotQ.Commands;

[Command("今日XX", "绑定今日XX", "今日")]
public class BindCommand : GroupCommand
{
    static readonly Random _random = new();
    readonly ILogger<BindCommand> _logger;
    readonly Database _db;

    public BindCommand(Database database, ILogger<BindCommand> logger)
    {
        _db = database;
        _logger = logger;
    }

    public override async Task ExecuteAsync(GroupMessageContext context)
    {
        Debug.Assert(context.Event != null);
        var type = context.Event.Message.GetPlainText().Trim()[2..];
        if (string.IsNullOrEmpty(type)) type = "老公";
        try
        {
            var user = await context.GetGroupMemberAsync(context.Event.UserId);
            var userModel = await _db.GetUserOrCreateAsync(user!.UserId.ToString(), user!.Nickname);
            string wifeNick;
            DateTime wifeExpire;
            long wifeId;
            if(userModel.Binded != null && userModel.Binded.Expire > DateTime.Now)
            {
                wifeExpire = userModel.Binded.Expire;
                wifeId = long.Parse(userModel.Binded.Ident);
                wifeNick = await context.GetUserNickAsync(wifeId) ?? "未知";
            }
            else
            {
                var randomMember = await GetRandomMemberAsync(context);
                if (randomMember == null) throw new Exception($"{user.UserId} is too unlucky! All three shots failed.");
                wifeExpire = await _db.BindUserAsync(userModel, randomMember.Item1);
                wifeNick = string.IsNullOrEmpty(randomMember.Item2.Card) 
                    ? randomMember.Item2.Nickname 
                    : randomMember.Item2.Card;
                wifeId = randomMember.Item2.UserId;
            }

            await context.Quick($"""
            [CQ:at,qq={context.Event.UserId}]
            喵喵给你找到了哦！🎉
            你的今日{type}素：
            [CQ:image,file=http://q.qlogo.cn/headimg_dl?dst_uin={wifeId}&spec=640&img_type=jpg]
            {wifeNick} ({wifeId})

            他会一直陪你到{wifeExpire:yyyy-MM-dd}~
            """);
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing Bind command");
            
        }
        await context.Quick(new Message($"你的{type}好像跑掉了……"));
    }

    async Task<Tuple<UserModel, GroupMemberInfoData>?> GetRandomMemberAsync(GroupMessageContext context)
    {
        if(context.Event == null)
        {
            _logger.LogError("Event did not injected.");
            return null;
        }
        var members = await context.GetGroupMemberListAsync();
        var availableMembers = members
            ?.Where(m => m.UserId != context.Event.SelfId && m.UserId != context.Event.UserId)
            .ToList();
        if (availableMembers?.Count == 0)
        {
            _logger.LogWarning($"No available members found in group {context.Event.GroupId} for random selection.");
            return null;
        }

        int i = 0;
        UserModel user;
        GroupMemberInfoData randomMember;
        do
        {
            randomMember = availableMembers![_random.Next(availableMembers.Count)]!;
            user = await _db.GetUserOrCreateAsync(randomMember.UserId.ToString(), randomMember.Nickname);
        } while (i < 3 && (user.Binded != null && user.Binded.Expire > DateTime.Now));

        return i<3 ? new Tuple<UserModel, GroupMemberInfoData>(user, randomMember) : null;
    }
}
