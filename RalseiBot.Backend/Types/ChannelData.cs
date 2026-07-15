using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types;

public enum ChannelType
{
    Text,
    Voice,
    Thread
}

public class ChannelData
{
    /// <summary>
    ///     The NAME of the channel.
    /// </summary>
    [JsonPropertyName("name")]
    public required string ChannelName { get; init; }

    /// <summary>
    ///     The ID of the channel.
    /// </summary>
    [JsonPropertyName("id")]
    public ulong ChannelId { get; init; }

    /// <summary>
    ///     The ID of the guild that the channel is in.
    /// </summary>
    [JsonPropertyName("guild_id")]
    public ulong GuildId { get; init; }

    /// <summary>
    ///     The type of the channel.
    /// </summary>
    [JsonPropertyName("channel_Type")]
    public ChannelType TypeChannel { get; init; }
}