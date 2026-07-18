using System.Data;
using MySqlConnector;
using NetCord.Rest;
using ralsei_bot_discord.Types.Database;

namespace ralsei_bot_discord.Controllers.Services;

public interface IserverdbService
{
    /// <summary>
    ///     Add entry to the database.
    /// </summary>
    /// <param name="guildData">The data to add to the server database.</param>
    public Task<DefaultResult> AddEntry(GuildData guildData);

    /// <summary>
    ///     Remove entry by the ID.
    /// </summary>
    /// <param name="id">The ID to reference.</param>
    public Task<DefaultResult> RemoveEntry(int id);

    /// <summary>
    ///     Removes entry by the Guild ID.
    /// </summary>
    /// <param name="guildId">The GUILD ID.</param>
    /// <returns></returns>
    public Task<DefaultResult> RemoveEntry(ulong guildId);

    /// <summary>
    ///     Get an entry by the ID.
    /// </summary>
    /// <param name="guildId">Guild ID.</param>
    /// <returns>The data containing the guild.</returns>
    public Task<GuildData?> GetEntryById(ulong guildId);

    /// <summary>
    ///     Get all the entries in the database.
    /// </summary>
    /// <returns>Entries.</returns>
    public Task<List<GuildData>> GetEntries();

    /// <summary>
    ///     Update the specified entry.
    /// </summary>
    /// <param name="guildData">The Guild Data to use when searching.</param>
    public Task<DefaultResult> UpdateEntry(GuildData guildData);
}

public class serverdbService(
    ILogger<serverdbService> logger,
    RestClient restClient,
    [FromKeyedServices("serverdb")] MySqlDataSource serverdbSource) : IserverdbService
{
    /// <summary>
    ///     Add an entry to the database.
    /// </summary>
    /// <param name="guildData">The data to add to the database.</param>
    public async Task<DefaultResult> AddEntry(GuildData guildData)
    {
        await using var connection = await serverdbSource.OpenConnectionAsync();

        // Insert the server database data.
        // If it exists, simply ignore the call and continue.
        var command =
            new MySqlCommand(
                "INSERT IGNORE INTO servers(guild_id, ralsei_channel_id, general_channel_id, moderation_channel_id) VALUES (@guild_id, @ralsei_channel_id, @general_channel_id, @moderation_channel_id)",
                connection);
        command.Parameters.AddWithValue("@guild_id", guildData.GuildId);
        command.Parameters.AddWithValue("@ralsei_channel_id", guildData.RalseiChannelId);
        command.Parameters.AddWithValue("@general_channel_id", guildData.GeneralChannelId);
        command.Parameters.AddWithValue("@moderation_channel_id", guildData.ModerationChannelId);
        var rowsAffected = await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();

        logger.LogInformation("Configuration done for guild ID: {Id}", guildData.GuildId);

        return new DefaultResult
        {
            Message = $"{rowsAffected} rows affected."
        };
    }

    /// <summary>
    ///     Remove the specified entry by ID.
    /// </summary>
    /// <param name="id">The ID to reference while deleting.</param>
    public async Task<DefaultResult> RemoveEntry(int id)
    {
        return await DeletionCommandWrapper("DELETE FROM servers WHERE id=@id", id);
    }

    /// <summary>
    ///     Remove the specified entry by GuildID.
    /// </summary>
    /// <param name="guildId">The GuildID to reference while deleting.</param>
    public async Task<DefaultResult> RemoveEntry(ulong guildId)
    {
        return await DeletionCommandWrapper("DELETE FROM servers WHERE guild_id=@id", guildId);
    }

    /// <summary>
    ///     Get the specified entry by the ID.
    /// </summary>
    /// <param name="guildId">The Guild ID reference.</param>
    /// <returns>The specified GuildData.</returns>
    public async Task<GuildData?> GetEntryById(ulong guildId)
    {
        await using var connection = await serverdbSource.OpenConnectionAsync();

        var guildInformation = await restClient.GetGuildAsync(guildId);

        var command = new MySqlCommand("SELECT * FROM servers WHERE guild_id=@guild_id;", connection);
        command.Parameters.AddWithValue("@guild_id", guildId);

        var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) return null;
        // Get all the database entries to the record.
        var guildData = new GuildData
        {
            Id = reader.GetInt32("id"),
            Name = guildInformation.Name,
            GuildId = await reader.IsDBNullAsync("guild_id") ? null : reader.GetUInt64("guild_id"),
            GeneralChannelId = await reader.IsDBNullAsync("general_channel_id")
                ? null
                : reader.GetUInt64("general_channel_id"),
            ModerationChannelId = await reader.IsDBNullAsync("moderation_channel_id")
                ? null
                : reader.GetUInt64("moderation_channel_id"),
            RalseiChannelId = await reader.IsDBNullAsync("ralsei_channel_id")
                ? null
                : reader.GetUInt64("ralsei_channel_id")
        };

        await connection.CloseAsync();
        return guildData;
    }

    /// <summary>
    ///     Get all the entries in the database.
    /// </summary>
    /// <returns>All the entries in the database.</returns>
    public async Task<List<GuildData>> GetEntries()
    {
        await using var connection = await serverdbSource.OpenConnectionAsync();

        var command = new MySqlCommand("SELECT * FROM servers;", connection);

        // Start reading from the database entries.
        var guildDataList = new List<GuildData>();
        var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var guildId = reader.GetUInt64("guild_id");
            var guildInformation = await restClient.GetGuildAsync(guildId);
            guildDataList.Add(new GuildData
            {
                Id = reader.GetInt32("id"),
                Name = guildInformation.Name,
                GeneralChannelId = reader.GetUInt64("general_channel_id"),
                GuildId = guildId,
                ModerationChannelId = reader.GetUInt64("moderation_channel_id"),
                RalseiChannelId = reader.GetUInt64("ralsei_channel_id")
            });
        }

        await connection.CloseAsync();
        return guildDataList;
    }

    public async Task<DefaultResult> UpdateEntry(GuildData guildData)
    {
        var isGuildIdNull = guildData.GuildId == null;

        await using var connection = await serverdbSource.OpenConnectionAsync();

        // Get based on ID or GuildID.
        var command =
            new MySqlCommand(
                $"UPDATE servers SET ralsei_channel_id=@ralsei_channel_id," +
                $"general_channel_id=@general_channel_id," +
                $"moderation_channel_id=@moderation_channel_id WHERE {(isGuildIdNull ? "id" : "guild_id")}=@id",
                connection);

        // Update the parameters based on the new data.
        command.Parameters.AddWithValue("@id", isGuildIdNull ? guildData.Id : guildData.GuildId);
        command.Parameters.AddWithValue("@general_channel_id", guildData.GeneralChannelId);
        command.Parameters.AddWithValue("@moderation_channel_id", guildData.ModerationChannelId);
        command.Parameters.AddWithValue("@ralsei_channel_id", guildData.RalseiChannelId);

        var results = await command.ExecuteNonQueryAsync();

        return new DefaultResult
        {
            Message = $"{results} rows affected."
        };
    }

    /// <summary>
    ///     Wrapper for the deletion command.
    /// </summary>
    /// <param name="commandString">The command to execute.</param>
    /// <param name="parameter">The parameter. In SQL params, this must be named "id"</param>
    private async Task<DefaultResult> DeletionCommandWrapper(string commandString, object parameter)
    {
        await using var connection = await serverdbSource.OpenConnectionAsync();

        var command = new MySqlCommand(commandString, connection);
        command.Parameters.AddWithValue("@id", parameter);
        var rowsAffected = await command.ExecuteNonQueryAsync();
        await connection.CloseAsync();

        return new DefaultResult
        {
            Message = $"{rowsAffected} rows affected."
        };
    }
}