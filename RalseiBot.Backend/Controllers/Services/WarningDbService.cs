using MySqlConnector;
using ralsei_bot_discord.Types.Database;

namespace ralsei_bot_discord.Controllers.Services;

public interface IWarningDbService
{
    /// <summary>
    ///     Add an entry to the database.
    /// </summary>
    /// <param name="warningData">The warning data.</param>
    public Task<DefaultResult> AddEntry(WarningData warningData);

    /// <summary>
    ///     Update an entry in the database.
    /// </summary>
    /// <param name="warningData">The warning data.</param>
    public Task<DefaultResult> UpdateEntry(WarningData warningData);

    /// <summary>
    ///     Delete an entry in the database.
    /// </summary>
    /// <param name="id">Id.</param>
    public Task<DefaultResult> DeleteEntryById(int id);

    /// <summary>
    ///     Get an entry by userId.
    /// </summary>
    /// <param name="userId">userId.</param>
    public Task<WarningData> GetEntryByUserId(ulong userId);

    /// <summary>
    ///     Get an entry by id.
    /// </summary>
    /// <param name="id">Id.</param>
    public Task<WarningData> GetEntryById(int id);

    /// <summary>
    ///     This will get all the current warnings in the database.
    /// </summary>
    /// <returns>Warnings.</returns>
    public Task<List<WarningData>> GetWarnings();
}

public class WarningDbService([FromKeyedServices("WarningDB")] MySqlDataSource warningDbSource) : IWarningDbService
{
    /// <summary>
    ///     Add an entry to the database.
    /// </summary>
    /// <param name="warningData">The warning data.</param>
    public async Task<DefaultResult> AddEntry(WarningData warningData)
    {
        await using var connection = await warningDbSource.OpenConnectionAsync();
        var command = new MySqlCommand("INSERT INTO users(user_id, warning_count) VALUES (@user_id, @warning_count)",
            connection);
        command.Parameters.AddWithValue("@user_id", warningData.UserId);
        command.Parameters.AddWithValue("@warning_count", warningData.WarningCount);

        var result = await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();

        return new DefaultResult
        {
            Message = $"{result} number of rows."
        };
    }

    /// <summary>
    ///     Update an entry in the database.
    /// </summary>
    /// <param name="warningData">The warning data.</param>
    public async Task<DefaultResult> UpdateEntry(WarningData warningData)
    {
        await using var connection = await warningDbSource.OpenConnectionAsync();
        var command =
            new MySqlCommand("UPDATE users SET user_id=@user_id, warning_count=@warning_count WHERE id=@id",
                connection);
        command.Parameters.AddWithValue("@user_id", warningData.UserId);
        command.Parameters.AddWithValue("@warning_count", warningData.WarningCount);

        var result = await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();

        return new DefaultResult
        {
            Message = $"{result} number of rows."
        };
    }

    /// <summary>
    ///     Delete an entry in the database.
    /// </summary>
    /// <param name="id">Id.</param>
    public async Task<DefaultResult> DeleteEntryById(int id)
    {
        await using var connection = await warningDbSource.OpenConnectionAsync();
        var command = new MySqlCommand("DELETE FROM users WHERE id=@id", connection);
        command.Parameters.AddWithValue("@id", id);

        var result = await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();

        return new DefaultResult
        {
            Message = $"{result} number of rows."
        };
    }

    /// <summary>
    ///     Get an entry by userId.
    /// </summary>
    /// <param name="userId">userId.</param>
    public async Task<WarningData> GetEntryByUserId(ulong userId)
    {
        return await GetWrapper(userId, "SELECT * FROM users WHERE user_id = @user_id");
    }

    /// <summary>
    ///     Get an entry by id.
    /// </summary>
    /// <param name="id">Id.</param>
    public async Task<WarningData> GetEntryById(int id)
    {
        return await GetWrapper(id, "SELECT * FROM users WHERE id = @id");
    }

    /// <summary>
    ///     This will get all the current warnings in the database.
    /// </summary>
    /// <returns>Warnings.</returns>
    public async Task<List<WarningData>> GetWarnings()
    {
        await using var connection = await warningDbSource.OpenConnectionAsync();
        var command = new MySqlCommand("SELECT * FROM users", connection);
        var result = await command.ExecuteReaderAsync();
        var warnings = new List<WarningData>();

        while (result.Read())
            warnings.Add(new WarningData
            {
                UserId = result.GetUInt64("user_id"),
                WarningCount = result.GetInt32("warning_count"),
                Id = result.GetInt32("id")
            });

        return warnings;
    }

    /// <summary>
    ///     This method gets the specified warning data by an ID reference...
    /// </summary>
    /// <param name="id">The ID reference.</param>
    /// <param name="commandWrapper">The command that will be executed.</param>
    /// <returns>The specified warning data.</returns>
    /// <exception cref="NullReferenceException"></exception>
    private async Task<WarningData> GetWrapper(object id, string commandWrapper)
    {
        await using var connection = await warningDbSource.OpenConnectionAsync();
        var command = new MySqlCommand(commandWrapper, connection);
        command.Parameters.AddWithValue("@id", id);

        var result = await command.ExecuteReaderAsync();
        if (result.Read())
            return new WarningData
            {
                UserId = result.GetUInt64("user_id"),
                WarningCount = result.GetInt32("warning_count"),
                Id = result.GetInt32("id")
            };
        throw new NullReferenceException("Couldn't find user!");
    }
}