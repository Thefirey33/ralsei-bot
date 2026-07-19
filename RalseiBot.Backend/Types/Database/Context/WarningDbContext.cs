using Microsoft.EntityFrameworkCore;

namespace ralsei_bot_discord.Types.Database.Context;

public class WarningDbContext(DbContextOptions<WarningDbContext> options) : DbContext(options)
{
    /// <summary>
    ///     Contains warning data.
    /// </summary>
    public DbSet<WarningData> WarningData { get; set; }
}