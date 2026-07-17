using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
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
builder.Services
    .AddSingleton<GeneralSharedState>()
    .AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "X-Access-Token";
        options.LoginPath = "/Login";
    });

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();
builder.Services.AddAuthorizationCore();
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddTransient<CookieForwardingHandler>();

builder.Services.AddHttpClient("RalseiBotBackend",
        client =>
        {
            client.BaseAddress = new Uri("https+http://RalseiBotBackend");
            // Allow a better time-out, because the Discord API is notoriously slow to respond at times.
            client.Timeout = TimeSpan.FromHours(5);
        })
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

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseRouting();
app.MapControllers();
app.UseWebSockets();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();