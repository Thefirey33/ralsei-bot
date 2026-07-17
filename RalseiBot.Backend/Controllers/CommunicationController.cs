using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetCord;
using NetCord.Rest;
using ralsei_bot_discord.Controllers.Services;
using ralsei_bot_discord.Handlers;
using ralsei_bot_discord.Types;
using ralsei_bot_discord.Types.Database;
using ralsei_bot_discord.Types.Requests;

namespace ralsei_bot_discord.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/messages")]
public class CommunicationController(
    RestClient restClient,
    ResponseSystemHandler systemHandler,
    ILogger<CommunicationController> logger,
    ICommunicationService communicationService) : ControllerBase
{
    /// <summary>
    ///     The maximum amount of messages that are fetched.
    /// </summary>
    private const int MaximumAmountMessages = 50;


    /// <summary>
    ///     Attempt to receive all the messages in the channel.
    ///     This route takes a maximum amount of requests. See <see cref="MaximumAmountMessages" />
    /// </summary>
    /// <param name="channelId"></param>
    /// <returns></returns>
    [HttpGet("{channelId:long}")]
    public async Task<IActionResult> GetChannelMessages(ulong channelId)
    {
        var channel = await restClient.GetChannelAsync(channelId) as TextGuildChannel;

        if (channel == null)
            return BadRequest(new DefaultResult
            {
                Message = "Channel doesn't exist!",
                StatusCode = 400
            });

        var fetchedMessages = restClient.GetMessagesAsync(channelId, new PaginationProperties<ulong>
            {
                Direction = PaginationDirection.Before
            })
            .Take(MaximumAmountMessages)
            .Reverse();


        // Filter all the messages into the MessageData system.
        return Ok(fetchedMessages.Select(message => MessageData.FromMessage(
            channel.GuildId, message)));
    }

    /// <summary>
    ///     Locks a channel.
    /// </summary>
    /// <param name="channelId">Channel ID.</param>
    [HttpPost("lock/{channelId:long}")]
    public async Task<IActionResult> LockChannel(ulong channelId)
    {
        while (true)
        {
            var channel = await restClient.GetChannelAsync(channelId) as TextGuildChannel;
            if (channel == null)
                return BadRequest(new DefaultResult
                {
                    Message = "Channel is not an appropriate channel type!",
                    StatusCode = 400
                });

            // Handle a very specific case where there's no permission overwrites, at all!
            if (!channel.PermissionOverwrites.TryGetValue(channel.GuildId, out var currentPermissions))
            {
                // If there's none, create some.
                await channel.ModifyPermissionsAsync(
                    new PermissionOverwriteProperties(channel.GuildId, PermissionOverwriteType.Role));
                channelId = channel.GuildId;
                continue;
            }

            // This operation allows the channel to get locked or unlocked.
            var currentAllowedPermissions = currentPermissions.Allowed;
            var currentDeniedPermissions = currentPermissions.Denied;

            // Toggle the state of the channel.
            var isExistsBoth = !(currentAllowedPermissions.HasFlag(Permissions.SendMessages) ||
                                 currentDeniedPermissions.HasFlag(Permissions.SendMessages));

            // Handle a very specific case.
            // The Discord API has a "no-mans-land" state, where both flags are not set.
            // When this happens, DO NOT TOGGLE THE ALLOWED PERMISSION.
            // Instead, set the denied permission instead.
            if (!isExistsBoth) currentAllowedPermissions ^= Permissions.SendMessages;

            if (currentAllowedPermissions.HasFlag(Permissions.SendMessages) || isExistsBoth)
                currentDeniedPermissions |= Permissions.SendMessages;
            else
                currentDeniedPermissions &= Permissions.SendMessages;

            var isChannelLocked = !currentAllowedPermissions.HasFlag(Permissions.SendMessages);

            // Send a message to the channel that informs the users that the state of the channel has been changed.
            await SendMessageToChannel(new MessageRequest
            {
                ChannelId = channelId,
                Message = systemHandler.GetRandomResponse(isChannelLocked
                    ? ResponseSystemHandler.ResponseTypes.LockChannel
                    : ResponseSystemHandler.ResponseTypes.UnlockChannel)
            });

            // Finally, modify it.
            await channel.ModifyPermissionsAsync(
                new PermissionOverwriteProperties(channel.GuildId, PermissionOverwriteType.Role)
                    { Allowed = currentAllowedPermissions, Denied = currentDeniedPermissions });

            return Ok();
        }
    }

    [HttpPost("purge")]
    public async Task<IActionResult> PurgeMessages(MessageRequest messageRequest)
    {
        logger.LogInformation("Preparing to purge messages in channel: {Channel}", messageRequest.ChannelId);
        if (messageRequest.Id == null)
            return BadRequest(new DefaultResult
            {
                Message = "ID not stated!",
                StatusCode = 400
            });

        // Retrieve the list of messages to be deleted.
        var messages = await restClient.GetMessagesAsync(messageRequest.ChannelId)
            .TakeWhile(message => message.Id != messageRequest.Id)
            .Select(message => message.Id)
            .ToListAsync();

        // Delete each message one by one.
        // This is a very basic purging system.
        await restClient.DeleteMessagesAsync(messageRequest.ChannelId, messages);

        return Ok();
    }


    /// <summary>
    ///     This allows the bot to send a message to the specified channel.
    /// </summary>
    /// <param name="messageRequest">The message request, request data.</param>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessageToChannel(MessageRequest messageRequest)
    {
        var result = await communicationService.SendMessageToChannel(messageRequest);
        return StatusCode(result.StatusCode, result.Message);
    }
}