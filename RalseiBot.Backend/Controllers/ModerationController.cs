using Microsoft.AspNetCore.Mvc;
using ralsei_bot_discord.Controllers.Services;
using ralsei_bot_discord.Types;

namespace ralsei_bot_discord.Controllers;

[ApiController]
[Route("[controller]/{guildId}")]
public class ModerationController(IModerationService moderationService) : ControllerBase
{
    /// <summary>
    ///     Kick a user.
    /// </summary>
    /// <param name="guildId">Guild ID.</param>
    /// <param name="userId">User ID.</param>
    [HttpPost("kick/{userId}")]
    public async Task<IActionResult> KickUser(ulong guildId, ulong userId)
    {
        var result = await moderationService.KickUser(guildId, userId);
        return StatusCode(result.StatusCode, result.Message);
    }

    /// <summary>
    ///     Ban a user.
    /// </summary>
    /// <param name="guildId">Guild ID.</param>
    /// <param name="userId">User ID.</param>
    [HttpPost("ban/{userId}")]
    public async Task<IActionResult> BanUser(ulong guildId, ulong userId)
    {
        var result = await moderationService.BanUser(guildId, userId);
        return StatusCode(result.StatusCode, result.Message);
    }

    [HttpPost("purge")]
    public async Task<IActionResult> Purge(ulong guildId, List<UserData> userData)
    {
        var result = await moderationService.PurgeEverything(userData, guildId);
        return StatusCode(result.StatusCode, result.Message);
    }
}