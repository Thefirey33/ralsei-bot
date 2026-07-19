using NetCord.Rest;
using ralsei_bot_discord.Handlers;
using ralsei_bot_discord.Types;
using ralsei_bot_discord.Types.Database;
using ralsei_bot_discord.Types.Requests;

namespace ralsei_bot_discord.Controllers.Services;

public interface IModerationService
{
    /// <summary>
    ///     The default reason.
    /// </summary>
    private const string DefaultReason = "The Ban Hammer Has Spoken!";

    /// <summary>
    ///     Kick user.
    /// </summary>
    /// <param name="guildId">Guild ID.</param>
    /// <param name="userId">User's ID.</param>
    /// <param name="reason">Reason.</param>
    public Task<DefaultResult> KickUser(ulong guildId, ulong userId, string reason = DefaultReason);

    /// <summary>
    ///     Ban user.
    /// </summary>
    /// <param name="guildId">Guild ID.</param>
    /// <param name="userId">User's ID.</param>
    /// <param name="reason">Reason.</param>
    public Task<DefaultResult> BanUser(ulong guildId, ulong userId, string reason = DefaultReason);

    /// <summary>
    ///     Literally purges everyone.
    /// </summary>
    /// <param name="userData">UserData(s)</param>
    /// <param name="guildId">Guild ID.</param>
    public Task<DefaultResult> PurgeEverything(List<UserData> userData, ulong guildId);
}

public class ModerationService(
    RestClient restClient,
    IServiceProvider serviceProvider,
    RandomQuoteHandler randomQuoteHandler,
    ILogger<ModerationService> logger) : IModerationService
{
    /// <summary>
    ///     Ban the specified user.
    /// </summary>
    /// <param name="guildId">Guild ID.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="reason">Reason</param>
    public async Task<DefaultResult> BanUser(ulong guildId, ulong userId, string reason)
    {
        await restClient.BanGuildUserAsync(guildId, userId);
        return await SendMessage(guildId, userId, RandomQuoteHandler.ResponseTypes.Banned, reason);
    }

    /// <summary>
    ///     Kick the specified user.
    /// </summary>
    /// <param name="guildId">Guild ID.</param>
    /// <param name="userId">User ID.</param>
    /// <param name="reason">Reason</param>
    public async Task<DefaultResult> KickUser(ulong guildId, ulong userId, string reason)
    {
        await restClient.KickGuildUserAsync(guildId, userId);
        return await SendMessage(guildId, userId, RandomQuoteHandler.ResponseTypes.Kicked, reason);
    }

    public async Task<DefaultResult> PurgeEverything(List<UserData> userData, ulong guildId)
    {
        // Ban everyone.
        // Kill everyone.
        // Kill Susie.
        foreach (var user in userData)
            try
            {
                await restClient.BanGuildUserAsync(guildId, user.Id);
            }
            catch (Exception e)
            {
                logger.LogWarning("Failed to ban user: {Id}. Reason: {Message}", user.Id, e.Message);
            }

        return new DefaultResult
        {
            Message = "Banned everyone."
        };
    }

    private async Task<DefaultResult> SendMessage(ulong guildId, ulong userId,
        RandomQuoteHandler.ResponseTypes responseType, string reason)
    {
        // Get server information.
        var scope = serviceProvider.CreateScope();
        var serverDbService = scope.ServiceProvider.GetRequiredService<ServerDbService>();
        var communicationService = scope.ServiceProvider.GetRequiredService<CommunicationService>();

        var entry = await serverDbService.GetEntryById(guildId);
        if (entry?.ModerationChannelId == null)
            return new DefaultResult
            {
                Message = "DB Entry doesn't exist!",
                StatusCode = 400
            };

        await communicationService.SendMessageToChannel(new MessageRequest
        {
            ChannelId = entry.ModerationChannelId.Value,
            Message = string.Format(randomQuoteHandler.GetRandomResponse(responseType),
                userId, reason)
        });

        return new DefaultResult
        {
            Message = "Successfully banned user!"
        };
    }
}