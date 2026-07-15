// JSON Structure for Messages.

using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types;

public class MessageData
{
    /// <summary>
    ///     The ID of the message.
    /// </summary>
    public ulong Id { get; init; }

    /// <summary>
    ///     The creator of the message.
    /// </summary>
    public required string Author { get; init; }

    /// <summary>
    ///     The TEXT of the message.
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text { get; init; }

    /// <summary>
    ///     The ID for the channel, for future references.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public ulong ChannelId { get; init; }

    /// <summary>
    ///     The time this message was created at.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     The profile picture URL of the user.
    /// </summary>
    [JsonPropertyName("pfp_url")]
    public string? ProfilePictureLink { get; init; }

    /// <summary>
    ///     What message is this message a reply to?
    /// </summary>
    public ulong? ReplyTo { get; init; }

    /// <summary>
    ///     The list of attachments this message has.
    /// </summary>
    public required List<AttachmentData> Attachments { get; init; }

    /// <summary>
    ///     All the embeds.
    /// </summary>
    public required List<EmbedData> Embeds { get; init; }
}