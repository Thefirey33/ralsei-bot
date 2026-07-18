using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;
using ralsei_bot_discord.Controllers.Services;
using ralsei_bot_discord.Types;
using ralsei_bot_discord.Types.Requests;

namespace ralsei_bot_discord.Handlers;

// ReSharper, this class isn't supposed to be instantiated.
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class GuildMessagesHandler(
    IHubContext<MessagingHub> context,
    ILogger<GuildMessagesHandler> logger,
    IHttpClientFactory httpClientFactory,
    RandomQuoteHandler randomQuoteHandler,
    ICommunicationService communicationService,
    IwarningdbService warningdbService,
    IModerationService moderationService,
    RestClient restClient)
    : IMessageCreateGatewayHandler, IMessageDeleteGatewayHandler, IMessageDeleteBulkGatewayHandler
{
    /// <summary>
    ///     The HTTP Client for communicating with the Python Filtering Service.
    /// </summary>
    private readonly HttpClient? _httpClient = httpClientFactory.CreateClient("ralseibotclassification");


    private readonly Dictionary<RandomQuoteHandler.ResponseTypes, List<string>> _keywords = new()
    {
        [RandomQuoteHandler.ResponseTypes.PetPet] = ["petpet", "patpat", "pet", "patpat"],
        [RandomQuoteHandler.ResponseTypes.CalledCute] = ["cute", "cutie", "cutes", "cutsies"],
        [RandomQuoteHandler.ResponseTypes.Boop] = ["boop"]
    };

    [Inject] public ICommunicationService CommunicationService { get; set; } = communicationService;

    /// <summary>
    ///     Handle the message creation handler.
    /// </summary>
    /// <param name="message">The new message.</param>
    public async ValueTask HandleAsync(Message message)
    {
        if (message.Channel is not TextGuildChannel || message.GuildId == null)
            return;

        // Send websocket request containing JSON data that contains the new message.
        var sendData =
            JsonSerializer.Serialize(MessageData.FromMessage(message.GuildId.Value, message));

        await context.Clients.All.SendAsync("MessageCreationSend", sendData);

        // Check for offensiveness or NSFW.
        logger.LogInformation("Checking {Id} for NSFW and offensiveness...", message.Id);
        if (_httpClient == null)
            return;

        var currentUser = await restClient
            .GetCurrentUserAsync();


        var result = await _httpClient.PostAsJsonAsync("/filter_text", new MessageTextRequest
        {
            Text = message.Content
        });
        var resultGraph = await result.Content.ReadFromJsonAsync<ResultCheck>();

        if (resultGraph == null)
        {
            await CheckResponse();
            return;
        }

        if (resultGraph.IsHateful || resultGraph.IsNsfw)
        {
            var warningCount = await warningdbService.IncrementWarningCount(message.Author.Id);

            // If the user is above 3 warnings, then ban them.
            if (warningCount >= 3 && message.Guild != null)
                await moderationService.KickUser(message.GuildId.Value, message.Author.Id, "Reached maximum warnings");

            // When it's detected that it's either, the message will be deleted.
            await CommunicationService.SendMessageToChannel(new MessageRequest
            {
                ChannelId = message.ChannelId,
                ResponseTo = message.Id,
                Message = randomQuoteHandler.GetRandomResponse(RandomQuoteHandler.ResponseTypes.RuleViolation)
            })!;

            await message.DeleteAsync();
        }

        await CheckResponse();
        return;

        async Task CheckResponse()
        {
            // Check if the message contains Ralsei.
            if (message.MentionedUsers.Any(user => user.Id == currentUser.Id)) await ResponseToMessages(message);
        }
    }

    /// <summary>
    ///     When messages are deleted in bulk, this handler sends a request to the socket to delete them.
    /// </summary>
    /// <param name="arg">The MessageDeleteBulk Event Arguments.</param>
    public async ValueTask HandleAsync(MessageDeleteBulkEventArgs arg)
    {
        await context.Clients.All.SendAsync("MessageDeletionBulkSend", arg.MessageIds);
    }

    /// <summary>
    ///     Sends a deletion message request.
    /// </summary>
    /// <param name="arg">Message Deletion Arguments</param>
    public async ValueTask HandleAsync(MessageDeleteEventArgs arg)
    {
        await context.Clients.All.SendAsync("MessageDeletionSend", arg.MessageId);
    }

    /// <summary>
    ///     Respond to the specified message if the response is detected.
    /// </summary>
    /// <param name="message">Message.</param>
    private async Task ResponseToMessages(Message message)
    {
        var text = message.Content;
        // Attempt to retrieve the type of the message.
        var responseCheck = _keywords.FirstOrDefault(pair =>
                pair.Value.Any(s => text.Contains(s, StringComparison.OrdinalIgnoreCase)))
            .Key;

        if (responseCheck == 0)
            return;

        await CommunicationService.SendMessageToChannel(new MessageRequest
        {
            ResponseTo = message.Id,
            ChannelId = message.ChannelId,
            Message = randomQuoteHandler.GetRandomResponse(responseCheck)
        });
    }
}

/// <summary>
///     The general communication socket.
/// </summary>
public class MessagingHub : Hub
{
    public async Task MessageCreationSend(string message)
    {
        await Clients.All.SendAsync("MessageCreationSend", message);
    }

    public async Task MessageDeletionSend(ulong id)
    {
        await Clients.All.SendAsync("MessageDeletionSend", id);
    }

    public async Task MessageDeletionBulkSend(List<ulong> messages)
    {
        await Clients.All.SendAsync("MessageDeletionSend", messages);
    }
}