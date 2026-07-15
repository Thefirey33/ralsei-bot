// JSON Structure for the bot's base information.

using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types;

public class GuildInformation
{
    /// <summary>
    ///     The name of the guild.
    /// </summary>
    [JsonPropertyName("guild_name")]
    public required string GuildName { get; init; }

    /// <summary>
    ///     The ID of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public ulong GuildId { get; init; }

    /// <summary>
    ///     Does the bot have permissions in the specified guild?
    ///     These permissions include moderation permissions, sending message permissions etc.
    /// </summary>
    [JsonPropertyName("has_perms")]
    public bool HasPermissions { get; init; }
}

public class InformationData
{
    /// <summary>
    ///     The total number of guilds that the bot is currently in.
    /// </summary>
    [JsonPropertyName("guild_data")]
    public int GuildCount { get; init; }

    /// <summary>
    ///     The names of all the guilds the bot is connected towards.
    /// </summary>
    public required List<GuildInformation> GuildInformation { get; init; }

    /// <summary>
    ///     The bot's name and ID together in a string.
    /// </summary>
    [JsonPropertyName("user_information")]
    public required string UserInformation { get; init; }
}