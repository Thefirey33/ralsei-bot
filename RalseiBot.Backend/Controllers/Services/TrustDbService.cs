using MySqlConnector;
using ralsei_bot_discord.Types.Database;

namespace ralsei_bot_discord.Controllers.Services;

public interface ItrustdbService
{
    /// <summary>
    ///     Get all the users in the trusted database.
    /// </summary>
    /// <returns>All the users in the trusted database.</returns>
    public Task<List<TrustData>> GetAllTrusts();

    /// <summary>
    ///     Posts a user to the trusted database.
    /// </summary>
    /// <param name="data">Request Data.</param>
    public Task PostUser(TrustRequestData data);

    /// <summary>
    ///     This deletes a user from the trusted database.
    /// </summary>
    /// <param name="data">Request Data</param>
    public Task DeleteUser(TrustRequestData data);

    /// <summary>
    ///     This checks if the specified user exists in the database.
    /// </summary>
    /// <param name="data">Request Data</param>
    public Task<bool> UserExistsInDb(TrustRequestData data);
}

public class trustdbService([FromKeyedServices("trustdb")] MySqlDataSource trustdbSource) : ItrustdbService
{
    /// <summary>
    ///     Get all the trusted users in the database.
    /// </summary>
    /// <returns>Trusted users.</returns>
    public async Task<List<TrustData>> GetAllTrusts()
    {
        await using var connection = await trustdbSource.OpenConnectionAsync();
        var trustData = new List<TrustData>();

        var command = new MySqlCommand("SELECT * FROM users;", connection);
        var readers = await command.ExecuteReaderAsync();

        while (await readers.ReadAsync())
            trustData.Add(new TrustData
            {
                Id = readers.GetInt32("id"),
                UserId = readers.GetInt64("user_id")
            });

        await connection.CloseAsync();

        return trustData;
    }

    /// <summary>
    ///     Post a user to the database.
    /// </summary>
    /// <param name="data">Request Data.</param>
    public async Task PostUser(TrustRequestData data)
    {
        await using var connection = await trustdbSource.OpenConnectionAsync();
        var command = new MySqlCommand("INSERT INTO users(user_id) VALUES (@user_id);", connection);
        command.Parameters.AddWithValue("@user_id", data.UserId);
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    /// <summary>
    ///     Delete a user from the database.
    /// </summary>
    /// <param name="data">Request Data.</param>
    public async Task DeleteUser(TrustRequestData data)
    {
        await using var connection = await trustdbSource.OpenConnectionAsync();
        var command = new MySqlCommand("DELETE FROM users WHERE user_id=@user_id;", connection);
        command.Parameters.AddWithValue("@user_id", data.UserId);
        await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();
    }

    /// <summary>
    ///     Check if a user exists in the database.
    /// </summary>
    /// <param name="data">Request Data.</param>
    public async Task<bool> UserExistsInDb(TrustRequestData data)
    {
        await using var connection = await trustdbSource.OpenConnectionAsync();
        var command = new MySqlCommand("SELECT count(*) FROM users WHERE user_id=@user_id");
        command.Parameters.AddWithValue("@user_id", data.UserId);

        // Check if there's any count of this user in the database.
        var result = Convert.ToInt32(await command.ExecuteScalarAsync());
        return result > 0;
    }
}