using Microsoft.AspNetCore.Mvc;
using NetCord;
using NetCord.Rest;
using ralsei_bot_discord.Types;

namespace ralsei_bot_discord.Controllers;

[ApiController]
[Route("[controller]")]
public class InformationController(RestClient restClient) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllInformation()
    {
        var currentUser = await restClient
            .GetCurrentUserAsync();

        var totalGuilds = await currentUser.GetGuildsAsync().ToListAsync();

        return Ok(new InformationData
        {
            GuildCount = totalGuilds.Count,
            UserInformation = $"{currentUser.Username}#{currentUser.Discriminator}",
            GuildInformation = totalGuilds.Select(guild => new GuildInformation
            {
                GuildName = guild.Name,
                GuildId = guild.Id,
                HasPermissions =
                    (guild.Permissions & Permissions.Administrator) != 0 // Does the bot have the specified permissions?
            }).ToList()
        });
    }
}