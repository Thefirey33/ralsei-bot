using System.Text.Json.Serialization;

namespace ralsei_bot_discord.Types.Database;

public struct ScoreData
{
    /// <summary>
    ///     The ID of this user in the scores.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }

    /// <summary>
    ///     The User Id of this user in the scores.
    ///     The User Id is the discord User Id of the user.
    /// </summary>
    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    /// <summary>
    ///     The Username of this user in the scores.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; }

    /// <summary>
    ///     Score of the user.
    /// </summary>
    [JsonPropertyName("score")]
    public int Score { get; set; }
}