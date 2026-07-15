using System.Text.Json.Serialization;
using NetCord;

namespace ralsei_bot_discord.Types;

public enum DataEmbedType
{
    [JsonPropertyName("rich")] Rich,

    [JsonPropertyName("image")] Image,

    [JsonPropertyName("video")] Video,

    [JsonPropertyName("gifv")] Gifv,

    [JsonPropertyName("article")] Article,

    [JsonPropertyName("link")] Link,

    [JsonPropertyName("poll_result")] PollResult
}

public class EmbedFieldData
{
    /// <summary>
    ///     The title of this field.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    ///     The value of this field.
    /// </summary>
    [JsonPropertyName("value")]
    public required string Value { get; set; }
}

public class EmbedData
{
    /// <summary>
    ///     The title of this embed.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    ///     The embed data type.
    /// </summary>
    public DataEmbedType? Type { get; set; }

    /// <summary>
    ///     The URL of this embed.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    ///     The fields of this embed.
    /// </summary>
    public required List<EmbedFieldData> Fields { get; set; }

    public static DataEmbedType FromEmbedType(EmbedType? embedType)
    {
        if (embedType == null)
            return 0;

        var enumValue = (int)embedType;
        return (DataEmbedType)enumValue;
    }
}