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

        var timeCalculation = (DateTime.Now + beforeSleepScheduleCalculation).Hour;

        if (timeCalculation is > SleepTime or < AwakeTime)
        {
            await gatewayClient.UpdatePresenceAsync(new PresenceProperties(UserStatusType.Idle)
                    .AddActivities(
                        new UserActivityProperties(
                            randomQuoteHandler.GetRandomResponse(RandomQuoteHandler.ResponseTypes.FluffySleep),
                            UserActivityType.Listening)),
                cancellationToken: stoppingToken);
            await Task.Delay(AwakeTime - DateTime.Now.Hour, stoppingToken);
        }
        else
        {
            // Update the activity of the bot based on a random activity value in the RandomActionsDay.json file.
            await gatewayClient.UpdatePresenceAsync(
                new PresenceProperties(UserStatusType.Online)
                    .AddActivities(
                        new UserActivityProperties(
                            randomQuoteHandler.GetRandomResponse(RandomQuoteHandler.ResponseTypes.RandomActivities),
                            UserActivityType.Playing)),
                cancellationToken: stoppingToken);

            await Task.Delay(
                beforeSleepScheduleCalculation,
                stoppingToken);
        }
    }
}