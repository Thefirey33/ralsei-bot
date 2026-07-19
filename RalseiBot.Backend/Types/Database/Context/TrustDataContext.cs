using Microsoft.EntityFrameworkCore;

namespace ralsei_bot_discord.Types.Database.Context;

public class TrustDataContext(DbContextOptions<TrustDataContext> options) : DbContext(options)
{
    /// <summary>
    ///     Contains trust data.
    /// </summary>
    public DbSet<TrustData> TrustData { get; set; }
}