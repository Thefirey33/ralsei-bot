using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types.Database;

public class TrustData
{
    /// <summary>
    ///     The ID of the user that's trusted.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>
    ///     The User ID of the user that's trusted.
    ///     The User ID is the discord user id.
    /// </summary>
    [JsonPropertyName("user_id")]
    public long UserId { get; init; }
}

public class TrustRequestData
{
    /// <summary>
    ///     The User ID of the user that's trusted.
    ///     The User ID is the discord user id.
    /// </summary>
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }
}