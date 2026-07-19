using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace ralsei_bot_discord.Controllers.Database;

/// <summary>
///     This checks the status of the database of the bot.
/// </summary>
/// <param name="serverdbSource">The serverdb, is where the server's individual channels are managed.</param>
/// <param name="trustdbSource">The trustdb, is where trusted users are added.</param>
/// <param name="scoredbSource">The scoredb, is where high-scores of games are stored.</param>
[ApiController]
[Authorize]
[Route("[controller]")]
public class DatabaseCheckerController(
    [FromKeyedServices("serverdb")] MySqlDataSource serverdbSource,
    [FromKeyedServices("trustdb")] MySqlDataSource trustdbSource,
    [FromKeyedServices("scoredb")] MySqlDataSource scoredbSource,
    [FromKeyedServices("warningdb")] MySqlDataSource warningdbSource) : ControllerBase
{
    /// <summary>
    ///     This function individually checks each database to make sure it works fine.
    /// </summary>
    /// <returns>The request containing data of the Database state.</returns>
    [HttpGet]
    public async Task<IActionResult> Ping()
    {
        // Check if all the servers are active.
        // Each server is pinged individually, and if the ping fails,
        // It will report an unhealthy status.

        await using var serverdbConnection = await serverdbSource.OpenConnectionAsync();
        await using var trustdbConnection = await trustdbSource.OpenConnectionAsync();
        await using var scoredbConnection = await scoredbSource.OpenConnectionAsync();
        await using var warningdbConnection = await warningdbSource.OpenConnectionAsync();

        // Ping each database and check individually for their connections.
        var isDatabaseAllActive = await serverdbConnection.PingAsync() && await trustdbConnection.PingAsync() &&
                                  await scoredbConnection.PingAsync() && await warningdbConnection.PingAsync();

        // Close the database connections.
        await trustdbConnection.CloseAsync();
        await serverdbConnection.CloseAsync();
        await scoredbConnection.CloseAsync();
        await warningdbConnection.CloseAsync();

        return Ok(isDatabaseAllActive);
    }
}