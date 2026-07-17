using System.ComponentModel.DataAnnotations;

namespace ralsei_bot_discord.Types.Requests;

public class LoginRequest
{
    /// <summary>
    /// </summary>
    [Required]
    public required string Username { get; set; }

    [Required] public required string Password { get; set; }
}