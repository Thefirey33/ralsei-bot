using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace ralsei_bot_discord.Handlers;

public class GuildMemberHandler(
    RestClient restClient,
    ILogger<GuildMemberHandler> logger)
    : IGuildUserAddGatewayHandler
{
    public async ValueTask HandleAsync(GuildUser arg)
    {
        if (arg.CreatedAt.AddYears(1) < DateTimeOffset.UtcNow) await restClient.KickGuildUserAsync(arg.GuildId, arg.Id);
    }
}