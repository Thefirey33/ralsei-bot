using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types.Database;

public class GuildData
{
    /// <summary>
    ///     The ID of this guild.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    ///     The Name of the guild.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     The ID of the guild.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public ulong? GuildId { get; set; }

    /// <summary>
    ///     The ID of the ralsei channel in the guild.
    /// </summary>
    [JsonPropertyName("ralsei_id")]
    public ulong? RalseiChannelId { get; set; }

    /// <summary>
    ///     The ID of the general channel in the guild.
    /// </summary>
    [JsonPropertyName("general_id")]
    public ulong? GeneralChannelId { get; set; }

    /// <summary>
    ///     The ID of the moderation channel in the guild.
    /// </summary>
    [JsonPropertyName("moderation_id")]
    public ulong? ModerationChannelId { get; set; }
}