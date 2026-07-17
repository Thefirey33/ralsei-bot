using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types.Database;

public class DefaultResult
{
    /// <summary>
    ///     The message string that is the default response message.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; set; }

    /// <summary>
    ///     The status code of the message.
    /// </summary>
    [JsonPropertyName("status_code")]
    public int StatusCode { get; set; } = 200;
}