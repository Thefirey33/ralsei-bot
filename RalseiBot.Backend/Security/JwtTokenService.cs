using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ralsei_bot_discord.Types.Requests;

namespace ralsei_bot_discord.Security;

[ApiController]
[Route("[controller]")]
public class JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger) : ControllerBase
{
    /// <summary>
    ///     The issuer, basically who issued the key.
    /// </summary>
    private readonly string _issuer =
        configuration["Authorization:Issuer"] ?? throw new SecurityException("Missing ISSUER");

    /// <summary>
    ///     The password hashing service that hashes the password for the admin.
    /// </summary>
    private readonly PasswordHasher<string> _passwordHasher = new();

    /// <summary>
    ///     The secret key of the authorization scheme.
    /// </summary>
    private readonly string _secretKey =
        configuration["Authorization:SecretKey"] ?? throw new SecurityException("Missing SECRET_KEY");

    /// <summary>
    ///     The maximum amount of time that the cookie and token will be stored for.
    /// </summary>
    private DateTime? MaxAuthTime => DateTime.UtcNow.AddHours(5);

    /// <summary>
    ///     This hashes a password.
    /// </summary>
    /// <param name="password">The password to hash.</param>
    /// <returns>Hashed password.</returns>
    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(string.Empty, password);
    }

    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(string.Empty, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success;
    }

    public string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.DateTime)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = MaxAuthTime,
            Issuer = _issuer,
            Audience = _issuer,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        if (!VerifyPassword(
                configuration["Authorization:AdminPassword"] ?? throw new SecurityException("No Admin Password!"),
                loginRequest.Password)) return Unauthorized();
        var token = GenerateJwtToken(loginRequest.Username);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = MaxAuthTime
        };

        logger.LogInformation("Processing dashboard sign in request...");
        HttpContext.Response.Cookies.Append("X-Access-Token", token, cookieOptions);

        return Ok();
    }
}