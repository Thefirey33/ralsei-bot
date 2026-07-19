using Microsoft.EntityFrameworkCore;
using ralsei_bot_discord.Types.Database;
using ralsei_bot_discord.Types.Database.Context;

namespace ralsei_bot_discord.Controllers.Services;

public interface ITrustDbService
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

public class TrustDbService(IServiceProvider serviceProvider) : ITrustDbService
{
    /// <summary>
    ///     Get all the trusted users in the database.
    /// </summary>
    /// <returns>Trusted users.</returns>
    public async Task<List<TrustData>> GetAllTrusts()
    {
        using var scope = serviceProvider.CreateScope();
        var trustDbSource = scope.ServiceProvider.GetRequiredService<TrustDataContext>();

        return await trustDbSource.TrustData.ToListAsync();
    }

    /// <summary>
    ///     Post a user to the database.
    /// </summary>
    /// <param name="data">Request Data.</param>
    public async Task PostUser(TrustRequestData data)
    {
        using var scope = serviceProvider.CreateScope();
        var trustDbSource = scope.ServiceProvider.GetRequiredService<TrustDataContext>();

        trustDbSource.TrustData.Add(new TrustData
        {
            UserId = data.UserId
        });
        await trustDbSource.SaveChangesAsync();
    }

    /// <summary>
    ///     Delete a user from the database.
    /// </summary>
    /// <param name="data">Request Data.</param>
    public async Task DeleteUser(TrustRequestData data)
    {
        using var scope = serviceProvider.CreateScope();
        var trustDbSource = scope.ServiceProvider.GetRequiredService<TrustDataContext>();

        var trustData = await trustDbSource.TrustData.FirstOrDefaultAsync(t => t.UserId == data.UserId);
        if (trustData == null)
            return;
        trustDbSource.TrustData.Remove(trustData);
        await trustDbSource.SaveChangesAsync();
    }

    /// <summary>
    ///     Check if a user exists in the database.
    /// </summary>
    /// <param name="data">Request Data.</param>
    public async Task<bool> UserExistsInDb(TrustRequestData data)
    {
        using var scope = serviceProvider.CreateScope();
        var trustDbSource = scope.ServiceProvider.GetRequiredService<TrustDataContext>();

        return await trustDbSource.TrustData.AnyAsync(t => t.UserId == data.UserId);
    }
}