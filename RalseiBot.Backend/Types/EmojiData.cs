using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types;

public class EmojiData
{
    /// <summary>
    ///     The ID of the Emoji.
    /// </summary>
    [JsonPropertyName("id")]
    public ulong? Id { get; set; }

    /// <summary>
    ///     The name of the emoji.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    ///     If the emoji is animated.
    /// </summary>
    [JsonPropertyName("animated")]
    public bool Animated { get; set; }

    /// <summary>
    ///     Return the Emoji as a string.
    /// </summary>
    /// <returns>Emoji as the discord emoji string.</returns>
    public string GetEmojiString()
    {
        return $"<{(Animated ? "a" : string.Empty)}:{Name}:{Id}>";
    }
}