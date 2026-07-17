using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types.Requests;

public class MessageRequest
{
    /// <summary>
    ///     The ID of the message.
    /// </summary>
    [JsonPropertyName("id")]
    public ulong? Id { get; set; }

    /// <summary>
    ///     The ID of the channel to send requests to.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public ulong ChannelId { get; set; }

    /// <summary>
    ///     The message to send requests with.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    /// <summary>
    ///     What this message is a response to?
    /// </summary>
    [JsonPropertyName("reply_id")]
    public ulong? ResponseTo { get; set; }


    public static MessageRequest FromMessageData(MessageData messageData)
    {
        return new MessageRequest
        {
            Id = messageData.Id,
            ChannelId = messageData.ChannelId,
            Message = messageData.Text
        };
    }
}