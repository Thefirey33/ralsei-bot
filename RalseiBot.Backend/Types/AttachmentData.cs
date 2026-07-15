using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types;

public class AttachmentData
{
    /// <summary>
    ///     The URL of the attachment.
    /// </summary>
    [JsonPropertyName("url")]
    public required string Url { get; init; }

    /// <summary>
    ///     The fileType of the attachment.
    /// </summary>
    [JsonPropertyName("file_type")]
    public required string FileType { get; init; }
}