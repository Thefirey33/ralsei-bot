// JSON Structure for Messages.

using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types;

public struct MessageData
{
    /// <summary>
    ///     The creator of the message.
    /// </summary>
    public string Author { get; init; }

    /// <summary>
    ///     The ID of the message.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    ///     The TEXT of the message.
    /// </summary>
    [JsonPropertyName("text")]
    public string Text { get; init; }

    /// <summary>
    ///     The ID for the channel, for future references.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public ulong ChannelId { get; init; }

    /// <summary>
    ///     The time this message was created at.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
}