using System.Text.RegularExpressions;

namespace RalseiBot.Web.Components;

public static partial class RegexHelper
{
    /// <summary>
    ///     This regex detects if an emoji exists.
    /// </summary>
    /// <returns>Regex</returns>
    [GeneratedRegex("<a?:.+?:\\d{17,21}>")]
    public static partial Regex EmojiRegex();

    /// <summary>
    ///     This Regex removes the tags in the emoji.
    /// </summary>
    /// <returns>Regex</returns>
    [GeneratedRegex("[<>]")]
    public static partial Regex EmojiTagRegex();
}