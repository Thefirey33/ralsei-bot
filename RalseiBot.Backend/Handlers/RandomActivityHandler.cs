using NetCord;
using NetCord.Gateway;

namespace ralsei_bot_discord.Handlers;

public class RandomActivityHandler(
    RandomQuoteHandler randomQuoteHandler,
    GatewayClient gatewayClient)
    : BackgroundService
{
    /// <summary>
    ///     The Maximum amount of time one action will take.
    /// </summary>
    private const int MaximumDelayActionInterval = 100;

    /// <summary>
    ///     The minimum of time one action will take.
    /// </summary>
    private const int MinimumDelayActionInterval = 10;

    /// <summary>
    ///     The hour when Ralsei goes to bed.
    /// </summary>
    private const int SleepTime = 23;

    /// <summary>
    ///     The hour when Ralsei is awake at.
    /// </summary>
    private const int AwakeTime = 12;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Update the presence of the bot in the specified random time.


        var beforeSleepScheduleCalculation =
            TimeSpan.FromMinutes(Random.Shared.Next(MinimumDelayActionInterval, MaximumDelayActionInterval));

        await gatewayClient.UpdatePresenceAsync(new PresenceProperties(UserStatusType.Online).AddActivities(new UserActivityProperties(randomQouteHandler.GetRandomResponse(RandomQouteHandler.ResponseTypes.RandomActivities), UserActivityType.Playing)), cancellationToken: stoppingToken);

        await Task.Delay(
            beforeSleepScheduleCalculation,
            stoppingToken);
    }
}