using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;
using ralsei_bot_discord.Controllers;
using ralsei_bot_discord.Controllers.Services;
using ralsei_bot_discord.Types.Database;

namespace ralsei_bot_discord.Handlers;

public class GuildDatabasePrepHandler(
    ILogger<GuildDatabasePrepHandler> logger,
    IserverdbService serverdbService,
    RestClient restClient)
    : IGuildCreateGatewayHandler, IGuildDeleteGatewayHandler
{
    /// <summary>
    ///     When the bot is added to the specified Guild, scan and configure it.
    /// </summary>
    /// <param name="arg">The GuildCreate Event Arguments.</param>
    public async ValueTask HandleAsync(GuildCreateEventArgs arg)
    {
        await ScanAndConfigureGuild(arg.GuildId);
    }

    /// <summary>
    ///     When the bot is removed from a guild, the guild will be removed from the database.
    /// </summary>
    /// <param name="arg">The deletion arguments.</param>
    public async ValueTask HandleAsync(GuildDeleteEventArgs arg)
    {
        GuildController.SetGuildCountChanged(true);
        logger.LogInformation("USER deleted from guild: {Id}, removing from DB...", arg.GuildId);
        await serverdbService.RemoveEntry(arg.GuildId);
    }

    /// <summary>
    ///     Scans and configures the specified guild.
    /// </summary>
    /// <param name="guildId">The specified Guild ID to search for.</param>
    private async Task ScanAndConfigureGuild(ulong guildId)
    {
        GuildController.SetGuildCountChanged(true);
        var channels = await restClient.GetGuildChannelsAsync(guildId);
        await serverdbService.AddEntry(new GuildData
        {
            GuildId = guildId,
            GeneralChannelId = ScanForChannel("general")?.Id,
            ModerationChannelId = ScanForChannel("moderation")?.Id,
            RalseiChannelId = ScanForChannel("ralsei")?.Id
        });
        return;

        IGuildChannel? ScanForChannel(string channelName)
        {
            return channels.FirstOrDefault(c => c.Name == channelName);
        }
    }
}