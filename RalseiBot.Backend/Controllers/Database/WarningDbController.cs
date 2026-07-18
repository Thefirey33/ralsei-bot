using Microsoft.AspNetCore.Mvc;
using ralsei_bot_discord.Controllers.Services;
using ralsei_bot_discord.Types.Database;

namespace ralsei_bot_discord.Controllers.Database;

[ApiController]
[Route("[controller]")]
public class warningdbController(IwarningdbService warningdbService) : ControllerBase
{
    /// <summary>
    ///     Add an entry to the database.
    /// </summary>
    /// <param name="warningData">The database data.</param>
    [HttpPost]
    public async Task<IActionResult> AddEntry(WarningData warningData)
    {
        return Ok(await warningdbService.AddEntry(warningData));
    }

    /// <summary>
    ///     Update an entry in the database.
    /// </summary>
    /// <param name="warningData">The database data.</param>
    [HttpPut]
    public async Task<IActionResult> UpdateEntry(WarningData warningData)
    {
        return Ok(await warningdbService.UpdateEntry(warningData));
    }

    /// <summary>
    ///     Delete an entry from the database by ID.
    /// </summary>
    /// <param name="id">ID reference.</param>
    [HttpDelete("delete/{userId}")]
    public async Task<IActionResult> DeleteEntry(ulong userId)
    {
        return Ok(await warningdbService.DeleteEntryById(userId));
    }

    /// <summary>
    ///     Get an entry by USER ID.
    /// </summary>
    /// <param name="userId">User ID reference.</param>
    [HttpGet("userId/{userId}")]
    public async Task<IActionResult> GetEntryByUserId(ulong userId)
    {
        return Ok(await warningdbService.GetEntryByUserId(userId));
    }

    /// <summary>
    ///     Get an entry by ID.
    /// </summary>
    /// <param name="id">ID reference.</param>
    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> GetEntryById(int id)
    {
        return Ok(await warningdbService.GetEntryById(id));
    }

    /// <summary>
    ///     Get all entries in the database.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetEntries()
    {
        return Ok(await warningdbService.GetWarnings());
    }
}