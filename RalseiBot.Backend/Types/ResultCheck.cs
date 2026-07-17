using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types;

public class ResultCheck
{
    /// <summary>
    ///     If the specified content is NSFW.
    /// </summary>
    [JsonPropertyName("is_nsfw")]
    public bool IsNsfw { get; set; }

    /// <summary>
    ///     If the specified content is HATEFUL/OFFENSIVE.
    /// </summary>
    [JsonPropertyName("is_hateful")]
    public bool IsHateful { get; set; }
}

public class MessageTextRequest
{
    /// <summary>
    ///     The content of the message.
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; set; }
}