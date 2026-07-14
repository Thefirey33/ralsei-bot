using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using ralsei_bot_discord.Types.Database;

namespace ralsei_bot_discord.Controllers.Database;

[ApiController]
[Route("[controller]")]
public class TrustDbController([FromKeyedServices("TrustDB")] MySqlDataSource trustDbSource) : ControllerBase
{
    /// <summary>
    ///     Get all the trusted users in the Database.
    /// </summary>
    /// <returns>A list of trusted users.</returns>
    [HttpGet]
    public async Task<IActionResult> GetAllTrusts()
    {
        var mySqlConnection = await trustDbSource.OpenConnectionAsync();

        var trustData = new List<TrustData>();

        var mySqlCommand = new MySqlCommand("SELECT * FROM users;", mySqlConnection);
        var readers = await mySqlCommand.ExecuteReaderAsync();

        while (await readers.ReadAsync())
            trustData.Add(new TrustData
            {
                Id = readers.GetInt32("id"),
                UserId = readers.GetInt64("user_id")
            });

        await mySqlConnection.CloseAsync();

        return Ok(trustData);
    }

    /// <summary>
    ///     Add a trusted user into the database.
    /// </summary>
    /// <param name="userId">The User ID of the trusted user.</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> PostUser(TrustRequestData userId)
    {
        var mySqlConnection = await trustDbSource.OpenConnectionAsync();
        var mySqlCommand = new MySqlCommand($"INSERT INTO users(user_id) VALUES ({userId.UserId});", mySqlConnection);
        var result = await mySqlCommand.ExecuteNonQueryAsync();
        await mySqlConnection.CloseAsync();

        return Ok(new DefaultResult
        {
            Message = $"{result} columns affected."
        });
    }

    /// <summary>
    ///     Delete something from the table that matches a specified id.
    /// </summary>
    /// <param name="id">The ID of the entry.</param>
    /// <returns>The number of columns affected.</returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteUser([FromBody] int id)
    {
        var mySqlConnection = await trustDbSource.OpenConnectionAsync();
        var mySqlCommand = new MySqlCommand($"DELETE FROM users WHERE id={id};", mySqlConnection);
        var result = await mySqlCommand.ExecuteNonQueryAsync();
        await mySqlConnection.CloseAsync();

        return Ok(new DefaultResult
        {
            Message = $"{result} columns affected."
        });
    }
}