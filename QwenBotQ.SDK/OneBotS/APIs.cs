using QwenBotQ.SDK.Messages;
using QwenBotQ.SDK.Models.OneBot.API;
using QwenBotQ.SDK.Models.OneBot.Events;

namespace QwenBotQ.SDK.OneBotS;

public partial class OneBot
{
    public async Task<SingleApiResponse<SendMessageData>> SendMsgAsync(object message, long? userId = null, long? groupId = null)
    {
        if (!(message is string || message is Message)) throw new ArgumentException("Message must be either string or Message object.");

        if ((userId == null && groupId == null) || (userId != null && groupId != null))
        {
            throw new ArgumentException("Must specify either userId or groupId.");
        }

        var sendParams = new
        {
            user_id = userId,
            group_id = groupId,
            message
        };

        return await CallApiAsync<SingleApiResponse<SendMessageData>>("send_msg_rate_limited", sendParams);
    }

    public async Task<ApiResponse> DeleteMsgAsync(int messageId)
    {
        return await CallApiAsync(
            "delete_msg_rate_limited",
            new { message_id = messageId }
        );
    }

    public async Task<SingleApiResponse<GetMessageData>> GetMsgAsync(int messageId)
    {
        return await CallApiAsync<SingleApiResponse<GetMessageData>>(
            "get_msg",
            new { message_id = messageId }
        );
    }

    public async Task<SingleApiResponse<ForwardMessageData>> GetForwardMsgAsync(string Id)
    {
        return await CallApiAsync<SingleApiResponse<ForwardMessageData>>(
            "get_forward_msg",
            new { id = Id }
        );
    }

    public async Task<ApiResponse> SendLikeAsync(long userId, int? times = null)
    {
        return await CallApiAsync(
            "send_like_rate_limited",
            new { user_id = userId, times }
        );
    }

    public async Task<ApiResponse> SetGroupKickAsync(long groupId, long userId, bool? rejectAddRequest = null)
    {
        return await CallApiAsync(
            "set_group_kick_rate_limited",
            new { group_id = groupId, user_id = userId, reject_add_request = rejectAddRequest }
        );
    }

    public async Task<ApiResponse> SetGroupBanAsync(long groupId, long userId, long? duration = null)
    {
        return await CallApiAsync(
            "set_group_ban_rate_limited",
            new { group_id = groupId, user_id = userId, duration }
        );
    }

    public async Task<ApiResponse> SetGroupAnonymousBanAsync(long groupId, string flag, int? duration = null)
    {
        return await CallApiAsync(
            "set_group_anonymous_ban_rate_limited",
            new { group_id = groupId, flag, duration }
        );
    }

    public async Task<ApiResponse> SetGroupWholeBanAsync(long groupId, bool? enable = null)
    {
        return await CallApiAsync(
            "set_group_whole_ban_rate_limited",
            new { group_id = groupId, enable }
        );
    }

    public async Task<ApiResponse> SetGroupAdminAsync(long groupId, long userId, bool? enable = null)
    {
        return await CallApiAsync(
            "set_group_admin_rate_limited",
            new { group_id = groupId, user_id = userId, enable }
        );
    }

    public async Task<ApiResponse> SetGroupAnonymousAsync(long groupId, bool? enable = null)
    {
        return await CallApiAsync(
            "set_group_anonymous_rate_limited",
            new { group_id = groupId, enable }
        );
    }

    public async Task<ApiResponse> SetGroupCardAsync(long groupId, long userId, string? card = null)
    {
        return await CallApiAsync(
            "set_group_card_rate_limited",
            new { group_id = groupId, user_id = userId, card }
        );
    }

    public async Task<ApiResponse> SetGroupNameAsync(long groupId, string? groupName = null)
    {
        return await CallApiAsync(
            "set_group_name_rate_limited",
            new { group_id = groupId, group_name = groupName }
        );
    }

    public async Task<ApiResponse> SetGroupLeaveAsync(long groupId, bool? isDismiss = null)
    {
        return await CallApiAsync(
            "set_group_leave_rate_limited",
            new { group_id = groupId, is_dismiss = isDismiss }
        );
    }

    public async Task<ApiResponse> SetGroupSpecialTitleAsync(long groupId, long userId, string? specialTitle = null, int? duration = null)
    {
        return await CallApiAsync(
            "set_group_special_title_rate_limited",
            new { group_id = groupId, user_id = userId, special_title = specialTitle, duration }
        );
    }

    public async Task<ApiResponse> SetFriendAddRequestAsync(string flag, bool? approve = null, string? remark = null)
    {
        return await CallApiAsync(
            "set_friend_add_request_rate_limited",
            new { flag, approve, remark }
        );
    }

    public async Task<ApiResponse> SetGroupAddRequestAsync(string flag, string subType, bool? approve = null, string? reason = null)
    {
        return await CallApiAsync(
            "set_group_add_request_rate_limited",
            new { flag, sub_type = subType, approve, reason }
        );
    }

    public async Task<SingleApiResponse<LoginInfoData>> GetLoginInfoAsync()
    {
        return await CallApiAsync<SingleApiResponse<LoginInfoData>>("get_login_info");
    }

    public async Task<SingleApiResponse<StrangerInfoData>> GetStrangerInfoAsync(long userId)
    {
        return await CallApiAsync<SingleApiResponse<StrangerInfoData>>("get_stranger_info", new { user_id = userId });
    }

    public async Task<MultipleApiResponse<FriendListData>> GetFriendListAsync()
    {
        return await CallApiAsync<MultipleApiResponse<FriendListData>>("get_friend_list");
    }

    public async Task<SingleApiResponse<GroupInfoData>> GetGroupInfoAsync(long groupId)
    {
        return await CallApiAsync<SingleApiResponse<GroupInfoData>>("get_group_info", new { group_id = groupId });
    }

    public async Task<MultipleApiResponse<GroupInfoData>> GetGroupListAsync()
    {
        return await CallApiAsync<MultipleApiResponse<GroupInfoData>>("get_group_list");
    }

    public async Task<SingleApiResponse<GroupMemberInfoData>> GetGroupMemberInfoAsync(long groupId, long userId)
    {
        return await CallApiAsync<SingleApiResponse<GroupMemberInfoData>>("get_group_member_info", new { group_id = groupId, user_id = userId });
    }

    public async Task<MultipleApiResponse<GroupMemberInfoData>>
        GetGroupMemberListAsync(long groupId)
    {
        return await CallApiAsync<MultipleApiResponse<GroupMemberInfoData>>("get_group_member_list", new { group_id = groupId });
    }

    public async Task<SingleApiResponse<GroupHonorInfoData>> GetGroupHonorInfoAsync(long groupId, string type)
    {
        return await CallApiAsync<SingleApiResponse<GroupHonorInfoData>>("get_group_honor_info", new { group_id = groupId, type });
    }

    public async Task<SingleApiResponse<CredentialsData>> GetCredentialsAsync(string? domain = null)
    {
        return await CallApiAsync<SingleApiResponse<CredentialsData>>("get_credentials", new { domain });
    }

    public async Task<SingleApiResponse<FileData>> GetRecordAsync(string file, string outFormat)
    {
        return await CallApiAsync<SingleApiResponse<FileData>>("get_record", new { file, out_format = outFormat });
    }

    public async Task<SingleApiResponse<FileData>> GetImageAsync(string file)
    {
        return await CallApiAsync<SingleApiResponse<FileData>>("get_image", new { file });
    }

    public async Task<SingleApiResponse<AvailabilityData>> CanSendImageAsync()
    {
        return await CallApiAsync<SingleApiResponse<AvailabilityData>>("can_send_image");
    }

    public async Task<SingleApiResponse<AvailabilityData>> CanSendRecordAsync()
    {
        return await CallApiAsync<SingleApiResponse<AvailabilityData>>("can_send_record");
    }

    public async Task<SingleApiResponse<StatusData>> GetStatusAsync()
    {
        return await CallApiAsync<SingleApiResponse<StatusData>>("get_status");
    }

    public async Task<SingleApiResponse<VersionData>> GetVersionInfoAsync()
    {
        return await CallApiAsync<SingleApiResponse<VersionData>>("get_version_info");
    }

    public async Task<ApiResponse> SetRestartAsync(int? delay)
    {
        return await CallApiAsync("set_restart_rate_limited", new { delay });
    }

    public async Task<ApiResponse> SetCleanCacheAsync()
    {
        return await CallApiAsync("set_clean_cache_rate_limited");
    }

    internal async Task QuickOperationAsync<T>(T context, object operation) where T : BaseEvent
    {
        var operationParams = new
        {
            context,
            operation
        };

        await CallApiAsync(".handle_quick_operation_rate_limited", operationParams);
    }
}
