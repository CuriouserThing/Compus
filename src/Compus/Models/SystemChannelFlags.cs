using System;

namespace Compus.Models;

/// <summary>
///     https://discord.com/developers/docs/resources/guild#guild-object-system-channel-flags
/// </summary>
[Flags]
public enum SystemChannelFlags
{
    SuppressJoinNotifications = 1 << 0,
    SuppressPremiumSubscriptions = 1 << 1,
    SuppressGuildReminderNotifications = 1 << 2,
    SuppressJoinNotificationReplies = 1 << 3,
}
