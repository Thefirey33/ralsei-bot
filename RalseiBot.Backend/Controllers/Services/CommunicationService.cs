using NetCord;
using NetCord.Rest;
using ralsei_bot_discord.Types.Database;
using ralsei_bot_discord.Types.Requests;

namespace ralsei_bot_discord.Controllers.Services;

public interface ICommunicationService
{
    /// <summary>
    ///     This sends/replies with a message to the chat.
    /// </summary>
    /// <param name="messageRequest">The Message Request.</param>
    public Task<DefaultResult> SendMessageToChannel(MessageRequest messageRequest);
}

public class CommunicationService(RestClient restClient, ILogger<CommunicationService> logger) : ICommunicationService
{
    /// <summary>
    ///     The key hitting penalty. Basically, how fast Ralsei will type.
    /// </summary>
    private const int KeyHittingPenalty = 10;


    public async Task<DefaultResult> SendMessageToChannel(MessageRequest messageRequest)
    {
        var channel = await restClient.GetChannelAsync(messageRequest.ChannelId) as TextGuildChannel;
        RestMessage? message = null;
        if (messageRequest.ResponseTo.HasValue)
            message = await restClient.GetMessageAsync(messageRequest.ChannelId, messageRequest.ResponseTo.Value);

        if (channel == null)
            return new DefaultResult
            {
                Message = "Channel is not an appropriate channel type!",
                StatusCode = 400
            };


        // Sometimes the discord API can take a shit.
        try
        {
            await restClient.TriggerTypingAsync(messageRequest.ChannelId);
        }
        catch (Exception e)
        {
            logger.LogWarning("Trigger typing failed: {Message}", e.Message);
        }

        var typingCalculation
            = messageRequest.Message.Length * KeyHittingPenalty;

        await Task.Delay(typingCalculation);

        // Attempt to respond to the message.
        if (message != null)
            await message.ReplyAsync(messageRequest.Message);
        else
            await channel.SendMessageAsync(messageRequest.Message);

        logger.LogInformation("Sent message {Message} to channel with ID: {Id}", messageRequest.Message,
            messageRequest.Id);

        return new DefaultResult
        {
            Message = "Message OK"
        };
    }
}