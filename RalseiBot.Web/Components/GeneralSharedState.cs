using ralsei_bot_discord.Types;

namespace RalseiBot.Web.Components;

public class GeneralSharedState
{
    /// <summary>
    ///     The current guilds that the bot contains.
    /// </summary>
    public List<GuildData> CurrentGuilds { get; private set; } = [];

    /// <summary>
    ///     This updates the value of the guild state in the API.
    /// </summary>
    /// <param name="guilds">The new list of guilds.</param>
    public void UpdateValue(List<GuildData>? guilds)
    {
        if (guilds != null) CurrentGuilds = guilds;
    }

    public async Task CheckIfImported(HttpClient httpClient)
    {
        if (CurrentGuilds.Count > 0)
            return;
        CurrentGuilds = await httpClient.GetFromJsonAsync<List<GuildData>>("Guild/all") ?? [];
    }
}