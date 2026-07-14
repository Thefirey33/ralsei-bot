using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetCord.Rest;
using ralsei_bot_discord.Types;

namespace ralsei_bot_discord.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class CommunicationController(RestClient restClient) : ControllerBase
{
    /// <summary>
    ///     The maximum batch size the discord API will supply.
    /// </summary>
    public const int MaximumBatchSize = 50;

    private readonly PaginationProperties<ulong> _paginationProperties = new()
    {
        BatchSize = MaximumBatchSize
    };

    /// <summary>
    ///     Attempt to receive all the messages in the channel.
    ///     Attention! This function only receives in maximum batches. See <see cref="MaximumBatchSize" />.
    /// </summary>
    /// <param name="channelId"></param>
    /// <returns></returns>
    [HttpGet("messages/{channelId:long}")]
    public async Task<IActionResult> GetChannelMessages(ulong channelId)
    {
        var fetchedMessages = await restClient.GetMessagesAsync(channelId, _paginationProperties).ToListAsync();

        // Filter all the messages into the MessageData system.
        return Ok(fetchedMessages.Select(message => new MessageData
        {
            ChannelId = message.ChannelId,
            CreatedAt = message.CreatedAt,
            Text = message.Content,
            Author = message.Author.Username
        }));
    }
}