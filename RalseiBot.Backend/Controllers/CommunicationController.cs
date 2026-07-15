using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetCord;
using NetCord.Rest;
using ralsei_bot_discord.Scoped;
using ralsei_bot_discord.Types;
using ralsei_bot_discord.Types.Database;
using ralsei_bot_discord.Types.Requests;

namespace ralsei_bot_discord.Controllers;

[ApiController]
[Authorize]
[Route("[controller]/messages")]
public class CommunicationController(
    RestClient restClient,
    ILogger<CommunicationController> logger,
    ResponseSystemManager systemManager,
    IHttpClientFactory clientFactory) : ControllerBase
{
    /// <summary>
    ///     The key hitting penalty. Basically, how fast Ralsei will type.
    /// </summary>
    private const int KeyHittingPenalty = 100;

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
    public IActionResult GetChannelMessages(ulong channelId)
    {
        var fetchedMessages = restClient.GetMessagesAsync(channelId, new PaginationProperties<ulong>
            {
                Direction = PaginationDirection.Before
            })
            .Take(MaximumAmountMessages)
            .Reverse();

        // Filter all the messages into the MessageData system.
        return Ok(fetchedMessages.Select(message => new MessageData
        {
            Id = message.Id,
            ReplyTo = message.ReferencedMessage?.Id,
            ChannelId = message.ChannelId,
            CreatedAt = message.CreatedAt,
            Text = message.Content,
            Author = message.Author.GlobalName ?? message.Author.Username,
            ProfilePictureLink = message.Author
                .GetAvatarUrl()?
                .ToString(),
            Attachments = message.Attachments
                .Select(attach => new AttachmentData
                {
                    Url = attach.Url,
                    FileType = attach.ContentType ?? "text/plain"
                })
                .ToList(),
            Embeds = message.Embeds.Select(embed =>
                {
                    return new EmbedData
                    {
                        Title = embed.Title,
                        Fields = embed.Fields.Select(field => new EmbedFieldData
                            {
                                Name = field.Name,
                                Value = field.Value
                            })
                            .ToList(),
                        Type = EmbedData.FromEmbedType(embed.Type),
                        Url = RetrieveLinkFromEmbeddedMedia(embed, embed.Type)
                    };
                })
                .ToList()
        }));

        // Attempt to retrieve the link from the embedded media.
        string? RetrieveLinkFromEmbeddedMedia(Embed embed, EmbedType? embedType)
        {
            return embedType switch
            {
                EmbedType.Image => embed.Image?.ProxyUrl,
                EmbedType.Video or EmbedType.Gifv => embed.Video?.ProxyUrl,
                _ => embed.Url
            };
        }
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
                return BadRequest(new DefaultResult { Message = "Channel is not an appropriate channel type!" });

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
                Message = systemManager.GetRandomResponse(isChannelLocked
                    ? ResponseSystemManager.ResponseTypes.LockChannel
                    : ResponseSystemManager.ResponseTypes.UnlockChannel)
            });

            // Finally, modify it.
            await channel.ModifyPermissionsAsync(
                new PermissionOverwriteProperties(channel.GuildId, PermissionOverwriteType.Role)
                    { Allowed = currentAllowedPermissions, Denied = currentDeniedPermissions });

            return Ok();
        }
    }

    /// <summary>
    ///     This allows the bot to send a message to the specified channel.
    /// </summary>
    /// <param name="messageRequest">The message request, request data.</param>
    [HttpPost("send")]
    public async Task<IActionResult> SendMessageToChannel(MessageRequest messageRequest)
    {
        var channel = await restClient.GetChannelAsync(messageRequest.ChannelId) as TextGuildChannel;
        if (channel == null)
            return BadRequest(new DefaultResult
            {
                Message = "Channel is not an appropriate channel type!"
            });


        // Make the bot look like it's typing a message.
        await restClient.TriggerTypingAsync(messageRequest.ChannelId);

        var typingCalculation
            = messageRequest.Message.Length * KeyHittingPenalty;

        await Task.Delay(typingCalculation);
        await channel.SendMessageAsync(messageRequest.Message);

        return Ok();
    }
}