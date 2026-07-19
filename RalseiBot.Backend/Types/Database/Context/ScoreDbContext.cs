using Microsoft.EntityFrameworkCore;

namespace ralsei_bot_discord.Types.Database.Context;

public class ScoreDbContext(DbContextOptions<ScoreDbContext> options) : DbContext(options)
{
    /// <summary>
    ///     Contains score data.
    /// </summary>
    public DbSet<ScoreData> ScoreData { get; set; }
}