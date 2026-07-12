using Microsoft.AspNetCore.Mvc;
using NetCord;
using NetCord.Rest;
using ralsei_bot_discord.Types;

namespace ralsei_bot_discord.Controllers;

[ApiController]
[Route("[controller]")]
public class GuildController(RestClient restClient) : ControllerBase
{
    /// <summary>
    ///     Attempt to retrieve all the guilds the bot has.
    /// </summary>
    /// <returns>List of guilds that the bot has.</returns>
    [HttpGet("all")]
    public async Task<IActionResult> GetAllGuilds()
    {
        var guilds = await restClient
            .GetCurrentUserGuildsAsync()
            .ToListAsync();

        var guildConversion = guilds.Select(guild => new GuildData
        {
            Id = guild.Id,
            Name = guild.Name
        }).ToList();

        return Ok(guildConversion);
    }

    /// <summary>
    ///     Attempt to get all the channels in the guild.
    /// </summary>
    /// <param name="guildId">The ID of the guild.</param>
    /// <returns>All the channels that are inside the guild.</returns>
    [HttpGet("channels/{guildId:long}")]
    public async Task<IActionResult> GetGuildChannels(ulong guildId)
    {
        var fetchedChannels = await restClient.GetGuildChannelsAsync(guildId);
        // Filter all the channels to the ChannelData system.
        return Ok(
            fetchedChannels
                .Where(channel => channel is not CategoryGuildChannel)
                .Select(channel => new ChannelData
                {
                    ChannelId = channel.Id,
                    ChannelName = channel.Name,
                    GuildId = channel.GuildId
                })
                .ToList());
    }
}