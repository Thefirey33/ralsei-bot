// Ralsei Bot Discord API 
// This is the side of the bot that communicates with the Discord API.

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Rest;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using ralsei_bot_discord.Controllers.Services;
using ralsei_bot_discord.Handlers;
using ralsei_bot_discord.Types.Database.Context;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add logging so we can log what the program is doing.

builder.Services
    .AddLogging()
    .AddAntiforgery();

builder.Logging
    .ClearProviders()
    .AddConsole();

// Add the websocketing service.
builder.Services.AddSignalR()
    .AddHubOptions<MessagingHub>(options => { options.MaximumReceiveMessageSize = 10 * 1024 * 1024; });

var serverVersion = new MySqlServerVersion(new Version(8, 0, 0));
// Register all the database contexts.
builder.AddMySqlDbContext<ScoreDbContext>("scoredb",
    configureDbContextOptions: options => { options.UseMySql(serverVersion); });
builder.AddMySqlDbContext<TrustDataContext>("trustdb",
    configureDbContextOptions: options => { options.UseMySql(serverVersion); });
builder.AddMySqlDbContext<GuildDbContext>("serverdb",
    configureDbContextOptions: options => { options.UseMySql(serverVersion); });
builder.AddMySqlDbContext<WarningDbContext>("warningdb",
    configureDbContextOptions: options => { options.UseMySql(serverVersion); });

// These are the shared services that each API controller will have.
builder.Services
    .AddSingleton<ITrustDbService, TrustDbService>() // The Trusted User Database, where the trusted users are stored.
    .AddSingleton<IServerDbService, ServerDbService>() // The server configuration database.
    .AddSingleton<IWarningDbService, WarningDbService>() // The warning/moderation database.
    .AddSingleton<IModerationService, ModerationService>() // The moderation service.
    .AddSingleton<ICommunicationService, CommunicationService>() // The communication service's handler.
    .AddSingleton<RandomQuoteHandler>(); // Displays the random activities that Ralsei might do everyday.

builder.Services.AddHostedService<RandomActivityHandler>();

builder.Services.AddHttpClient("ralseibotclassification",
    client => { client.BaseAddress = new Uri("http+https://ralseibotclassification"); });

// Security
// This is handled using a JWT Bearer Token system, where a cookie will store the specified access token.
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Since this bot will run locally...
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Authorization:SecretKey"]!)),
            ValidIssuer = builder.Configuration["Authorization:Issuer"],
            ValidAudience = builder.Configuration["Authorization:Issuer"]
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("X-Access-Token", out var token)) context.Token = token;

                return Task.CompletedTask;
            }
        };
    });

// Register all the discord and API related services.
// Don't forget to register the Discord Rest API, it's the most important out of all of them.

builder.Services
    .AddDiscordGateway(options =>
    {
        options.Intents = GatewayIntents.All;
        options.Presence = new PresenceProperties(UserStatusType.Online);
    })
    .AddDiscordRest();

builder.Services
    .AddApplicationCommands()
    .AddOpenApi();

builder.Services
    .AddGatewayHandlers(typeof(Program).Assembly)
    .AddEndpointsApiExplorer()
    .AddControllers();

var app = builder.Build();

// WebSockets, because the interface needs to update in realtime.
app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(10) });

// Register the remaining controllers and modules that make up Ralsei.
app.MapDefaultEndpoints();
app.AddModules(typeof(Program).Assembly);

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// Use Swagger to make it easier to communicate with the API.
app.UseSwaggerUI(options => { options.SwaggerEndpoint("/openapi/v1.json", "RalseiAPI"); });

app.UseHttpsRedirection();
app.MapHub<MessagingHub>("/Communication");
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseHsts();
app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    // TODO: This is a more temporary solution until database migration is added.

    var scoreDbContext = scope.ServiceProvider.GetRequiredService<ScoreDbContext>();
    await scoreDbContext.Database.EnsureCreatedAsync();

    var warningDbContext = scope.ServiceProvider.GetRequiredService<WarningDbContext>();
    await warningDbContext.Database.EnsureCreatedAsync();

    var trustDbContext = scope.ServiceProvider.GetRequiredService<TrustDataContext>();
    await trustDbContext.Database.EnsureCreatedAsync();

    var guildDbContext = scope.ServiceProvider.GetRequiredService<GuildDbContext>();
    await guildDbContext.Database.EnsureCreatedAsync();
}

app.Run();