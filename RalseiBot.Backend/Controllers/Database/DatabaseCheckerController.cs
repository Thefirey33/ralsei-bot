using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace ralsei_bot_discord.Controllers.Database;

/// <summary>
///     This checks the status of the database of the bot.
/// </summary>
/// <param name="serverDbSource">The ServerDB, is where the server's individual channels are managed.</param>
/// <param name="trustDbSource">The TrustDB, is where trusted users are added.</param>
/// <param name="scoreDbSource">The ScoreDB, is where high-scores of games are stored.</param>
[ApiController]
[Authorize]
[Route("[controller]")]
public class DatabaseCheckerController(
    [FromKeyedServices("ServerDB")] MySqlDataSource serverDbSource,
    [FromKeyedServices("TrustDB")] MySqlDataSource trustDbSource,
    [FromKeyedServices("ScoreDB")] MySqlDataSource scoreDbSource,
    [FromKeyedServices("WarningDB")] MySqlDataSource warningDbSource) : ControllerBase
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

        await using var serverDbConnection = await serverDbSource.OpenConnectionAsync();
        await using var trustDbConnection = await trustDbSource.OpenConnectionAsync();
        await using var scoreDbConnection = await scoreDbSource.OpenConnectionAsync();
        await using var warningDbConnection = await warningDbSource.OpenConnectionAsync();

        // Ping each database and check individually for their connections.
        var isDatabaseAllActive = await serverDbConnection.PingAsync() && await trustDbConnection.PingAsync() &&
                                  await scoreDbConnection.PingAsync() && await warningDbConnection.PingAsync();

        // Close the database connections.
        await trustDbConnection.CloseAsync();
        await serverDbConnection.CloseAsync();
        await scoreDbConnection.CloseAsync();
        await warningDbConnection.CloseAsync();

        return Ok(isDatabaseAllActive);
    }
}