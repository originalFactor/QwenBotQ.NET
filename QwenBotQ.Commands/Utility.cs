using QwenBotQ.SDK.Core;
using Microsoft.Extensions.Logging;
using QwenBotQ.SDK.Models.Database;
using QwenBotQ.SDK.Models.OneBot.API;
using QwenBotQ.SDK.Context;
using QwenBotQ.SDK.DatabaseS;

namespace QwenBotQ.Commands;

internal static class Utility
{
    static public async Task<GroupMemberInfoData?> GetGroupMemberAsync(this GroupMessageContext context, long userId)
    {
        var members = await context.Bot.GetGroupMemberInfoAsync(context.Event!.GroupId, userId);
        return members.Data;
    }

    static public async Task<GroupMemberInfoData[]?> GetGroupMemberListAsync(this GroupMessageContext context)
    {
        var resp = await context.Bot.GetGroupMemberListAsync(context.Event!.GroupId);
        return resp.Data?.ToArray();
    }

    static public async Task<string?> GetUserNickAsync(this MessageContext context, long userId, bool preferCard = true)
    {
        if (context is GroupMessageContext groupContext)
        {
            var member = await groupContext.GetGroupMemberAsync(userId);
            if (member != null)
            {
                return preferCard && !string.IsNullOrEmpty(member.Card) ? member.Card : member.Nickname;
            }
        }
        var stranger = (await context.Bot.GetStrangerInfoAsync(userId))?.Data;
        return stranger?.Nickname;
    }

    static public async Task<UserModel?> GetSenderUserAsync(this MessageContext context, Database db)
    {
        if (context.Event == null) throw new InvalidOperationException("Event is not set for this context.");
        var userId = context.Event.UserId;
        var userNick = await context.GetUserNickAsync(userId, false);
        if (userNick == null) return null;
        return await db.GetUserOrCreateAsync(userId.ToString(), userNick);
    }
}
