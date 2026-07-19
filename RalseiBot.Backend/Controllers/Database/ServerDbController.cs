using Microsoft.AspNetCore.Mvc;
using ralsei_bot_discord.Controllers.Services;
using ralsei_bot_discord.Types.Database;

namespace ralsei_bot_discord.Controllers.Database;

[ApiController]
[Route("[controller]")]
public class ServerDbController(IServerDbService serverDbService) : ControllerBase
{
    /// <summary>
    ///     Add an entry to the database.
    /// </summary>
    /// <param name="guildData">The data to add the database.</param>
    [HttpPost("entry")]
    public async Task<IActionResult> AddEntry(GuildData guildData)
    {
        return Ok(await serverDbService.AddEntry(guildData));
    }

    /// <summary>
    ///     Remove the specified entry from the database.
    /// </summary>
    /// <param name="id">The ID of the entry.</param>
    [HttpDelete("entry/{id:int}")]
    public async Task<IActionResult> RemoveEntry(int id)
    {
        return Ok(await serverDbService.RemoveEntry(id));
    }

    /// <summary>
    ///     Get the specified entry by the GuildID.
    ///     If the entry doesn't exist, it will throw a 404 error.
    /// </summary>
    /// <param name="guildId">The specified guild that this entry is attached to.</param>
    [HttpGet("entry/{guildId}")]
    public async Task<IActionResult> GetEntryById(ulong guildId)
    {
        var result = await serverDbService.GetEntryById(guildId);
        if (result == null)
            return NotFound(new DefaultResult
            {
                Message = "No entry found!",
                StatusCode = 404
            });

        return Ok(result);
    }

    [HttpPost("entry/update")]
    public async Task<IActionResult> UpdateEntry(GuildData guildData)
    {
        var result = await serverDbService.UpdateEntry(guildData);
        return Ok(result);
    }

    /// <summary>
    ///     Get all the entries in the database.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetEntries()
    {
        return Ok(await serverDbService.GetEntries());
    }
}