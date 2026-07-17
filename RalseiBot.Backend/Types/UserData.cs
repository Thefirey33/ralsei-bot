using System.Text.Json.Serialization;
using NetCord;

namespace ralsei_bot_discord.Types;

public class UserData
{
    /// <summary>
    ///     The ID of the user.
    /// </summary>
    [JsonPropertyName("id")]
    public ulong Id { get; set; }

    /// <summary>
    ///     The username of the user.
    /// </summary>
    [JsonPropertyName("username")]
    public required string Username { get; set; }

    /// <summary>
    ///     The display name of the user.
    ///     If the display name is null, then the API will return the USERNAME of the user.
    /// </summary>
    [JsonPropertyName("display_name")]
    public string DisplayName
    {
        get => field ?? Username;
        init;
    }

    /// <summary>
    ///     Attempt to retrieve user data from the user.
    /// </summary>
    /// <param name="user">The user</param>
    /// <returns>UserData</returns>
    public static UserData GetFromUser(User user)
    {
        return new UserData
        {
            DisplayName = user.GlobalName,
            Id = user.Id,
            Username = user.Username
        };
    }
}