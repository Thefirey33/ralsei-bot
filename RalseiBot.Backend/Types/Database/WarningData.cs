using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types.Database;

public class WarningData
{
    /// <summary>
    ///     The ID Of this entry in the warning database.
    /// </summary>
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    /// <summary>
    ///     The user ID of the user.
    /// </summary>
    [JsonPropertyName("user_id")]
    public ulong UserId { get; set; }

    /// <summary>
    ///     The amount of times the user was warned.
    /// </summary>
    [JsonPropertyName("warning_count")]
    public int WarningCount { get; set; }
}