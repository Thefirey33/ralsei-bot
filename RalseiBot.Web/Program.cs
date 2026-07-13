using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using ralsei_bot_discord.Security;
using RalseiBot.Web.Components;
using RalseiBot.Web.Components.Security;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddLogging();

builder.Logging
    .ClearProviders()
    .AddConsole();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "X-Access-Token";
        options.LoginPath = "/Login";
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationCore();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTransient<CookieForwardingHandler>();

builder.Services.AddHttpClient("RalseiBotBackend",
        client => { client.BaseAddress = new Uri("https+http://RalseiBotBackend"); })
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        UseCookies = false
    })
    .AddHttpMessageHandler<CookieForwardingHandler>();

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", true);
    app.UseHsts();
}

// Map the login route...
// This allows the user to log in to the panel.
app.MapPost("/login-auth", async (
    HttpContext httpContext,
    IHttpClientFactory clientFactory,
    [FromForm] string username,
    [FromForm] string password,
    [FromForm] string? returnUrl) =>
{
    var httpClient = clientFactory.CreateClient("RalseiBotBackend");

    var loginPayload = new { Username = username, Password = password };
    var response = await httpClient.PostAsJsonAsync("JwtTokenService/login", loginPayload);

    if (!response.IsSuccessStatusCode) return Results.Redirect("/Login?error=InvalidCredentials");

    if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
        return Results.Redirect("/Login?error=TokenMissing");
    var tokenCookie = cookies.FirstOrDefault(c => c.StartsWith("X-Access-Token="));
    if (tokenCookie == null) return Results.Redirect("/Login?error=TokenMissing");
    var tokenValue = tokenCookie.Split(';')[0].Replace("X-Access-Token=", "");

    var claims = new List<Claim>
    {
        new(ClaimTypes.Name, username),
        new("RawJwtToken", tokenValue)
    };

    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

    await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        new ClaimsPrincipal(claimsIdentity),
        new AuthenticationProperties { IsPersistent = true }
    );

    return Results.Redirect(!string.IsNullOrEmpty(returnUrl) ? returnUrl : "/");
}).DisableAntiforgery();

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseRouting();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();