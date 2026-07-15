using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types.Requests;

public struct MessageRequest
{
    /// <summary>
    ///     The ID of the channel to send requests to.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public ulong ChannelId { get; set; }

    /// <summary>
    ///     The message to send requests with.
    /// </summary>
    [JsonPropertyName("message")]
    public string Message { get; set; }
}