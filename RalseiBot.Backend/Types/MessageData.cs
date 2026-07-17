// JSON Structure for Messages.

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using NetCord;
using NetCord.Rest;

namespace ralsei_bot_discord.Types;

public sealed class MessageData : INotifyPropertyChanged
{
    public enum ActionType
    {
        Delete,
        Create
    }

    /// <summary>
    ///     The ID of the message.
    /// </summary>
    public ulong Id { get; init; }

    /// <summary>
    ///     The ID of the guild that is currently holding this message.
    /// </summary>
    public ulong GuildId { get; init; }

    /// <summary>
    ///     The creator of the message.
    /// </summary>
    public required UserData Author { get; init; }

    /// <summary>
    ///     The TEXT of the message.
    /// </summary>
    [JsonPropertyName("text")]
    public required string Text
    {
        get;

        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    /// <summary>
    ///     The ID for the channel, for future references.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public ulong ChannelId { get; init; }

    /// <summary>
    ///     The time this message was created at.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    ///     The profile picture URL of the user.
    /// </summary>
    [JsonPropertyName("pfp_url")]
    public string? ProfilePictureLink { get; init; }

    /// <summary>
    ///     What message is this message a reply to?
    /// </summary>
    public ulong? ReplyTo { get; init; }

    /// <summary>
    ///     The list of attachments this message has.
    /// </summary>
    public required List<AttachmentData> Attachments { get; init; }

    /// <summary>
    ///     All the embeds.
    /// </summary>
    public required List<EmbedData> Embeds { get; init; }

    /// <summary>
    ///     The message's action type.
    /// </summary>
    [JsonPropertyName("action")]
    public ActionType MessageAction { get; set; } = ActionType.Create;

    public event PropertyChangedEventHandler? PropertyChanged;

    public static MessageData FromMessage(ulong guildId, RestMessage? message)
    {
        // Retrieve specified channel information.
        // This will come into handy when retrieving the Guild ID etc.
        ArgumentNullException.ThrowIfNull(message);

        return new MessageData
        {
            Id = message.Id,
            ReplyTo = message.ReferencedMessage?.Id,
            GuildId = guildId,
            ChannelId = message.ChannelId,
            CreatedAt = message.CreatedAt,
            Text = message.Content,
            Author = UserData.GetFromUser(message.Author),
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
        };

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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}