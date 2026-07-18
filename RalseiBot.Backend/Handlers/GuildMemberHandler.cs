using NetCord;
using NetCord.Hosting.Gateway;
using ralsei_bot_discord.Controllers.Services;
using ralsei_bot_discord.Types.Database;

namespace ralsei_bot_discord.Handlers;

public class GuildMemberHandler(
    IModerationService moderationService,
    ITrustDbService trustDbService,
    ILogger<GuildMemberHandler> logger)
    : IGuildUserAddGatewayHandler
{
    public async ValueTask HandleAsync(GuildUser arg)
    {
        // Check the joining user for several conditions.

        if (await trustDbService.UserExistsInDb(new TrustRequestData
            {
                UserId = Convert.ToInt64(arg.Id)
            }))
        {
            logger.LogInformation("User {userId} is already trusted, skipping.", arg.Id);
            return;
        }

        // Checks to see if it has an avatar, or it's account is eligible enough.
        if (arg.CreatedAt.AddYears(1) <= DateTime.Now)
            await moderationService.KickUser(arg.GuildId, arg.Id, "Account Too New...");

        if (!arg.HasAvatar)
            await moderationService.KickUser(arg.GuildId, arg.Id, "Account Too Suspicious...");
    }
}