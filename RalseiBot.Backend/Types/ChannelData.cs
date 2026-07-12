using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types;

public struct ChannelData
{
    /// <summary>
    ///     The NAME of the channel.
    /// </summary>
    [JsonPropertyName("name")]
    public string ChannelName { get; init; }

    /// <summary>
    ///     The ID of the channel.
    /// </summary>
    [JsonPropertyName("id")]
    public ulong ChannelId { get; init; }

    /// <summary>
    ///     The ID of the guild that the channel is in.
    /// </summary>
    [JsonPropertyName("guildId")]
    public ulong GuildId { get; init; }
}