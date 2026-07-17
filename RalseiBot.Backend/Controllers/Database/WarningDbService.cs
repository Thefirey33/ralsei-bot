using Microsoft.AspNetCore.Mvc;
using ralsei_bot_discord.Controllers.Services;
using ralsei_bot_discord.Types.Database;

namespace ralsei_bot_discord.Controllers.Database;

[ApiController]
[Route("[controller]")]
public class WarningDbService(IWarningDbService warningDbService) : ControllerBase
{
    /// <summary>
    ///     Add an entry to the database.
    /// </summary>
    /// <param name="warningData">The database data.</param>
    [HttpPost]
    public async Task<IActionResult> AddEntry(WarningData warningData)
    {
        return Ok(await warningDbService.AddEntry(warningData));
    }

    /// <summary>
    ///     Update an entry in the database.
    /// </summary>
    /// <param name="warningData">The database data.</param>
    [HttpPut]
    public async Task<IActionResult> UpdateEntry(WarningData warningData)
    {
        return Ok(await warningDbService.UpdateEntry(warningData));
    }

    /// <summary>
    ///     Delete an entry from the database by ID.
    /// </summary>
    /// <param name="id">ID reference.</param>
    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        return Ok(await warningDbService.DeleteEntryById(id));
    }

    /// <summary>
    ///     Get an entry by USER ID.
    /// </summary>
    /// <param name="userId">User ID reference.</param>
    [HttpGet("userId/{userId}")]
    public async Task<IActionResult> GetEntryByUserId(ulong userId)
    {
        return Ok(await warningDbService.GetEntryByUserId(userId));
    }

    /// <summary>
    ///     Get an entry by ID.
    /// </summary>
    /// <param name="id">ID reference.</param>
    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> GetEntryById(int id)
    {
        return Ok(await warningDbService.GetEntryById(id));
    }

    /// <summary>
    ///     Get all entries in the database.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetEntries()
    {
        return Ok(await warningDbService.GetWarnings());
    }
}