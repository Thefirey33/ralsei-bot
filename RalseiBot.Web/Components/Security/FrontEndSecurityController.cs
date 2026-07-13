using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace RalseiBot.Web.Components.Security;

[ApiController]
[Route("[controller]")]
public class FrontEndSecurityController(
    IHttpClientFactory clientFactory,
    IHttpContextAccessor httpContextAccessor)
    : ControllerBase
{
    private readonly HttpClient _httpClient = clientFactory.CreateClient("RalseiBotBackend");
    private readonly HttpContext? _httpContext = httpContextAccessor.HttpContext;

    /// <summary>
    ///     Delete the specified cookie.
    /// </summary>
    /// <returns>Redirect to the login.</returns>
    [HttpPost("Logout")]
    public Task<IResult> DeleteCookie()
    {
        try
        {
            Response.Cookies.Delete("X-Access-Token");

            return Task.FromResult(Results.Redirect("/Login"));
        }
        catch (Exception exception)
        {
            return Task.FromException<IResult>(exception);
        }
    }

    [HttpPost("Login")]
    public async Task<IResult> GetToken(
        [FromForm] string username,
        [FromForm] string password,
        [FromForm] string? returnUrl)
    {
        var loginPayload = new { Username = username, Password = password };
        var response = await _httpClient.PostAsJsonAsync("JwtTokenService/login", loginPayload);

        // If the credentials check failed, don't allow the user to enter.
        if (!response.IsSuccessStatusCode) return Results.Redirect("/Login?Error=InvalidCredentials");

        // If the token is missing, point it out.
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return Results.Redirect("/Login?Error=TokenMissing");

        // Extract the token from the game.
        var tokenCookie = cookies.FirstOrDefault(c => c.StartsWith("X-Access-Token="));
        if (tokenCookie == null) return Results.Redirect("/Login?Error=TokenMissing");
        var tokenValue = tokenCookie.Split(';')[0].Replace("X-Access-Token=", "");

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
            new("Token", tokenValue)
        };

        // Create the claims identity for the authentication engine.
        // After that, log into the game session.
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        if (_httpContext != null)
            await _httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties { IsPersistent = true }
            );

        return Results.Redirect(!string.IsNullOrEmpty(returnUrl) ? returnUrl : "/");
    }
}