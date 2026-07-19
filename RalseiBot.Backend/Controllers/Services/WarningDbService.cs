using Microsoft.EntityFrameworkCore;
using ralsei_bot_discord.Types.Database;
using ralsei_bot_discord.Types.Database.Context;

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
    public Task<DefaultResult> DeleteEntryById(ulong userId);

    /// <summary>
    ///     Get an entry by userId.
    /// </summary>
    /// <param name="userId">userId.</param>
    public Task<WarningData?> GetEntryByUserId(ulong userId);

    /// <summary>
    ///     Get an entry by id.
    /// </summary>
    /// <param name="id">Id.</param>
    public Task<WarningData?> GetEntryById(int id);

    /// <summary>
    ///     This will get all the current warnings in the database.
    /// </summary>
    /// <returns>Warnings.</returns>
    public Task<List<WarningData>> GetWarnings();

    /// <summary>
    ///     Increment a count of the user in the database.
    /// </summary>
    /// <param name="userId">ID of the user.</param>
    public Task<int> IncrementWarningCount(ulong userId);
}

public class WarningDbService(
    ILogger<WarningDbService> logger,
    IServiceProvider serviceProvider) : IWarningDbService
{
    /// <summary>
    ///     Add an entry to the database.
    /// </summary>
    /// <param name="warningData">The warning data.</param>
    public async Task<DefaultResult> AddEntry(WarningData warningData)
    {
        using var scope = serviceProvider.CreateScope();
        var warningDbSource = scope.ServiceProvider.GetRequiredService<WarningDbContext>();

        logger.LogInformation("ADD: {UserId}'s warning count is now {Count}", warningData.UserId,
            warningData.WarningCount);

        await warningDbSource.WarningData.AddAsync(warningData);
        await warningDbSource.SaveChangesAsync();

        return new DefaultResult
        {
            Message = "Done."
        };
    }

    /// <summary>
    ///     Update an entry in the database.
    /// </summary>
    /// <param name="warningData">The warning data.</param>
    public async Task<DefaultResult> UpdateEntry(WarningData warningData)
    {
        using var scope = serviceProvider.CreateScope();
        var warningDbSource = scope.ServiceProvider.GetRequiredService<WarningDbContext>();

        var result = await warningDbSource.WarningData.FindAsync(warningData.UserId, warningData.Id);
        if (result == null)
            return new DefaultResult
            {
                Message = "Could not find warning!",
                StatusCode = 404
            };
        warningDbSource.WarningData.Update(result);
        await warningDbSource.SaveChangesAsync();

        return new DefaultResult
        {
            Message = "Done."
        };
    }

    /// <summary>
    ///     Get an entry by userId.
    /// </summary>
    /// <param name="userId">userId.</param>
    public async Task<WarningData?> GetEntryByUserId(ulong userId)
    {
        using var scope = serviceProvider.CreateScope();
        var warningDbSource = scope.ServiceProvider.GetRequiredService<WarningDbContext>();

        return await warningDbSource.WarningData.FirstOrDefaultAsync(data => data.UserId == userId);
    }

    /// <summary>
    ///     Get an entry by id.
    /// </summary>
    /// <param name="id">Id.</param>
    public async Task<WarningData?> GetEntryById(int id)
    {
        using var scope = serviceProvider.CreateScope();
        var warningDbSource = scope.ServiceProvider.GetRequiredService<WarningDbContext>();

        return await warningDbSource.WarningData.FirstOrDefaultAsync(data => data.Id == id);
    }

    /// <summary>
    ///     This will get all the current warnings in the database.
    /// </summary>
    /// <returns>Warnings.</returns>
    public async Task<List<WarningData>> GetWarnings()
    {
        using var scope = serviceProvider.CreateScope();
        var warningDbSource = scope.ServiceProvider.GetRequiredService<WarningDbContext>();

        return await warningDbSource.WarningData.ToListAsync();
    }

    /// <summary>
    ///     This increments the warning count of one user.
    /// </summary>
    /// <param name="userId">The userId of the user.</param>
    public async Task<int> IncrementWarningCount(ulong userId)
    {
        using var scope = serviceProvider.CreateScope();
        var warningDbSource = scope.ServiceProvider.GetRequiredService<WarningDbContext>();

        var result = await warningDbSource.WarningData.FirstOrDefaultAsync(data => data.UserId == userId);
        if (result == null)
        {
            await warningDbSource.WarningData.AddAsync(new WarningData
            {
                UserId = userId,
                WarningCount = 1
            });
            return 1;
        }

        result.WarningCount++;
        warningDbSource.Update(result);
        await warningDbSource.SaveChangesAsync();

        return result.WarningCount;
    }

    /// <summary>
    ///     Delete an entry in the database.
    /// </summary>
    /// <param name="userId">Id.</param>
    public async Task<DefaultResult> DeleteEntryById(ulong userId)
    {
        using var scope = serviceProvider.CreateScope();
        var warningDbSource = scope.ServiceProvider.GetRequiredService<WarningDbContext>();

        var result = await warningDbSource.WarningData.FirstOrDefaultAsync(data => data.UserId == userId);
        if (result == null)
            return new DefaultResult
            {
                Message = "Didn't modify anything."
            };

        warningDbSource.WarningData.Remove(result);
        await warningDbSource.SaveChangesAsync();
        return new DefaultResult
        {
            Message = "Done."
        };
    }
}