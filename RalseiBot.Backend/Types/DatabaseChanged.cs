using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types;

public class DatabaseChanged
{
    /// <summary>
    ///     Indicator if the database property changed.
    /// </summary>
    [JsonPropertyName("changed")]
    public bool Changed { get; init; }
}