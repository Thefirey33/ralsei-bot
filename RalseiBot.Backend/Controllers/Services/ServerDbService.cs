using Microsoft.EntityFrameworkCore;
using ralsei_bot_discord.Types.Database;
using ralsei_bot_discord.Types.Database.Context;

namespace ralsei_bot_discord.Controllers.Services;

public interface IServerDbService
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

public class ServerDbService(IServiceProvider serviceProvider) : IServerDbService
{
    /// <summary>
    ///     Add an entry to the database.
    /// </summary>
    /// <param name="guildData">The data to add to the database.</param>
    public async Task<DefaultResult> AddEntry(GuildData guildData)
    {
        using var scope = serviceProvider.CreateScope();
        var serverDbSource = scope.ServiceProvider.GetRequiredService<GuildDbContext>();

        if (serverDbSource.GuildData.Any(g => g.GuildId == guildData.GuildId))
            return new DefaultResult
            {
                Message = "Already exists!",
                StatusCode = 302
            };

        await serverDbSource.GuildData.AddAsync(guildData);
        await serverDbSource.SaveChangesAsync();

        return new DefaultResult
        {
            Message = "Done."
        };
    }

    /// <summary>
    ///     Remove the specified entry by ID.
    /// </summary>
    /// <param name="id">The ID to reference while deleting.</param>
    public async Task<DefaultResult> RemoveEntry(int id)
    {
        using var scope = serviceProvider.CreateScope();
        var serverDbSource = scope.ServiceProvider.GetRequiredService<GuildDbContext>();

        var result = await serverDbSource.GuildData.FindAsync(id);
        if (result == null)
            return new DefaultResult
            {
                Message = "Didn't modify anything."
            };
        serverDbSource.GuildData.Remove(result);
        await serverDbSource.SaveChangesAsync();
        return new DefaultResult
        {
            Message = "Done."
        };
    }

    /// <summary>
    ///     Remove the specified entry by GuildID.
    /// </summary>
    /// <param name="guildId">The GuildID to reference while deleting.</param>
    public async Task<DefaultResult> RemoveEntry(ulong guildId)
    {
        using var scope = serviceProvider.CreateScope();
        var serverDbSource = scope.ServiceProvider.GetRequiredService<GuildDbContext>();

        var result = await serverDbSource.GuildData.FirstOrDefaultAsync(g => g.GuildId == guildId);
        if (result == null)
            return new DefaultResult
            {
                Message = "Didn't modify anything."
            };
        serverDbSource.GuildData.Remove(result);
        await serverDbSource.SaveChangesAsync();
        return new DefaultResult
        {
            Message = "Done."
        };
    }

    /// <summary>
    ///     Get the specified entry by the ID.
    /// </summary>
    /// <param name="guildId">The Guild ID reference.</param>
    /// <returns>The specified GuildData.</returns>
    public async Task<GuildData?> GetEntryById(ulong guildId)
    {
        using var scope = serviceProvider.CreateScope();
        var serverDbSource = scope.ServiceProvider.GetRequiredService<GuildDbContext>();

        return await serverDbSource.GuildData.FirstOrDefaultAsync(g => g.GuildId == guildId);
    }

    /// <summary>
    ///     Get all the entries in the database.
    /// </summary>
    /// <returns>All the entries in the database.</returns>
    public async Task<List<GuildData>> GetEntries()
    {
        using var scope = serviceProvider.CreateScope();
        var serverDbSource = scope.ServiceProvider.GetRequiredService<GuildDbContext>();

        return await serverDbSource.GuildData.ToListAsync();
    }

    public async Task<DefaultResult> UpdateEntry(GuildData guildData)
    {
        using var scope = serviceProvider.CreateScope();
        var serverDbSource = scope.ServiceProvider.GetRequiredService<GuildDbContext>();

        var result = await serverDbSource.GuildData.FirstOrDefaultAsync(g => g.GuildId == guildData.GuildId);
        if (result == null)
            return new DefaultResult
            {
                Message = "Didn't modify anything."
            };
        serverDbSource.GuildData.Update(result);
        await serverDbSource.SaveChangesAsync();

        return new DefaultResult
        {
            Message = "Done."
        };
    }
}