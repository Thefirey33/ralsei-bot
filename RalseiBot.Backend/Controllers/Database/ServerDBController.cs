using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace ralsei_bot_discord.Controllers.Database;

[ApiController]
[Authorize]
[Route("[controller]")]
public class ServerDbController([FromKeyedServices("ServerDB")] MySqlDataSource serverDbSource) : ControllerBase
{
    [HttpGet("count")]
    public async Task<IActionResult> GetCount()
    {
        await using var connection = await serverDbSource.OpenConnectionAsync();

        var countCommand = new MySqlCommand("SELECT * FROM servers", connection);
        var value = await countCommand.ExecuteScalarAsync();

        await connection.CloseAsync();

        return Ok(value);
    }
}