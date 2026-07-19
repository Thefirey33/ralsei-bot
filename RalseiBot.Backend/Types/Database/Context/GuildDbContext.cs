using Microsoft.EntityFrameworkCore;

namespace ralsei_bot_discord.Types.Database.Context;

public class GuildDbContext(DbContextOptions<GuildDbContext> options) : DbContext(options)
{
    /// <summary>
    ///     Contains guild data.
    /// </summary>
    public DbSet<GuildData> GuildData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuildData>(builder => { builder.HasIndex(data => data.GuildId).IsUnique(); });
    }
}