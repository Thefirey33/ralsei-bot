using System.ComponentModel.DataAnnotations;

namespace ralsei_bot_discord.Types.Requests;

public class LoginRequest
{
    /// <summary>
    /// </summary>
    [Required]
    public string Username { get; set; }

    [Required] public string Password { get; set; }
}