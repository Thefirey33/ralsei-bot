// JSON Structure for Guilds.

using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types;

public struct GuildData
{
    /// <summary>
    ///     The ID of the guild.
    /// </summary>
    [JsonPropertyName("id")]
    public ulong Id { get; init; }

    /// <summary>
    ///     The name of the guild.
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; init; }
}