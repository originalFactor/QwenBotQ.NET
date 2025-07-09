using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QwenBotQ.SDK.Events;
using QwenBotQ.SDK.Models;
using QwenBotQ.SDK.Core;
using Microsoft.Extensions.Logging;

namespace QwenBotQ.Commands
{
    internal static class Utility
    {
        static readonly Random _random = new();

        static public async Task<GroupMemberData?> GetGroupMemberAsync(IBotSDK sdk, long groupId, long userId)
        {
            var members = await sdk.OneBotService.GetGroupMemberInfoAsync(groupId, userId);
            return ((JObject?)members?.Data)?.ToObject<GroupMemberData>();
        }
        static public async Task<List<GroupMemberData>?> GetGroupMemberListAsync(IBotSDK sdk, long groupId)
        {
            var resp = await sdk.OneBotService.GetGroupMemberListAsync(groupId);
            return ((JArray?)resp.Data)?.ToObject<List<GroupMemberData>>();
        }

        static public async Task<UserModel> GetUserOrCreateAsync(GroupMemberData member, IBotSDK sdk, ILogger logger)
        {
            var user = await sdk.DataBaseService.GetUserAsync(member.UserId.ToString());
            if (user == null)
            {
                user = new UserModel
                {
                    Id = member.UserId.ToString(),
                    Nick = member.Nickname
                };
                await sdk.DataBaseService.SaveUserAsync(user);
                logger.LogInformation($"Created new user {user.Id} with nickname {user.Nick}");
            }
            if (string.IsNullOrEmpty(user.Nick))
            {
                user.Nick = member.Nickname;
                await sdk.DataBaseService.SaveUserAsync(user);
                logger.LogInformation($"Saved user {user.Id} with nickname {user.Nick}");
            }
            user.Nick = string.IsNullOrEmpty(member.Card) ? member.Nickname : member.Card;
            return user;
        }

        static public async Task<DateTime> BindUserAsync(IBotSDK sdk, ILogger logger, GroupMemberData user1d, GroupMemberData user2d)
        {
            var user1 = await GetUserOrCreateAsync(user1d, sdk, logger);
            var user2 = await GetUserOrCreateAsync(user2d, sdk, logger);

            var expire = DateTime.Today.AddDays(1);

            // Perform the binding logic here
            user1.Binded = new BindedModel
            {
                Ident = user2.Id,
                Expire = expire
            };
            user1.Binded = new BindedModel
            {
                Ident = user2.Id,
                Expire = expire
            };

            await sdk.DataBaseService.SaveUserAsync(user1);
            await sdk.DataBaseService.SaveUserAsync(user2);
            return expire;
        }
    }

    class GroupMemberData
    {
        [JsonProperty("user_id")]
        public required long UserId { get; set; }
        [JsonProperty("nickname")]
        public required string Nickname { get; set; }
        [JsonProperty("card")]
        public string? Card { get; set; }
    }
}
