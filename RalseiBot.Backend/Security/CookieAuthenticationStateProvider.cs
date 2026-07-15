using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ralsei_bot_discord.Security;

public class CookieAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var identity = new ClaimsIdentity();
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            var tokenString = httpContext.User.FindFirstValue("Token");
            if (!string.IsNullOrEmpty(tokenString))
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(tokenString))
                {
                    var jwtToken = handler.ReadJwtToken(tokenString);

                    if (DateTime.UtcNow > jwtToken.ValidTo)
                        return Task.FromResult(
                            new AuthenticationState(
                                new ClaimsPrincipal())); // If the user's token expired, kick em' out!

                    identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                }
            }
        }

        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }
}