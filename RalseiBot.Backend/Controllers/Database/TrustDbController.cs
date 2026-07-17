using Microsoft.AspNetCore.Mvc;
using ralsei_bot_discord.Controllers.Services;
using ralsei_bot_discord.Types.Database;

namespace ralsei_bot_discord.Controllers.Database;

[ApiController]
[Route("[controller]")]
public class TrustDbController(ITrustDbService service)
    : ControllerBase
{
    /// <summary>
    ///     Get all the trusted users in the Database.
    /// </summary>
    /// <returns>A list of trusted users.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllTrusts()
    {
        var result = await service.GetAllTrusts();
        return Ok(result);
    }

    /// <summary>
    ///     Add a trusted user into the database.
    /// </summary>
    /// <param name="data">Request Data</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> PostUser(TrustRequestData data)
    {
        await service.PostUser(data);
        return Ok();
    }

    /// <summary>
    ///     Delete something from the table that matches a specified id.
    /// </summary>
    /// <param name="data">Request Data</param>
    /// <returns>The number of columns affected.</returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteUser(TrustRequestData data)
    {
        await service.DeleteUser(data);
        return Ok();
    }
}