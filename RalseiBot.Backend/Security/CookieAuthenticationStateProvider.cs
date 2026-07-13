using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ralsei_bot_discord.Security;

public class CookieAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor) : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var context = httpContextAccessor.HttpContext;
        var identity = new ClaimsIdentity();

        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
            return Task.FromResult(new AuthenticationState(httpContext.User));

        var tokenString = context?.Request.Cookies["X-Access-Token"];

        if (!string.IsNullOrEmpty(tokenString))
        {
            var handler = new JwtSecurityTokenHandler();
            if (handler.CanReadToken(tokenString))
            {
                var jwtToken = handler.ReadJwtToken(tokenString);
                identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            }
        }

        var user = new ClaimsPrincipal(identity);
        return Task.FromResult(new AuthenticationState(user));
    }
}