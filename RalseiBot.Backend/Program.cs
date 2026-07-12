// Ralsei Bot Discord API 
// This is the side of the bot that communicates with the Discord API.

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Rest;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add logging so we can log what the program is doing.

builder.Services.AddLogging();

builder.Logging
    .ClearProviders()
    .AddConsole();

// Register all the discord and API related services.
// Don't forget to register the Discord Rest API, it's the most important out of all of them.

builder.Services
    .AddDiscordGateway(options =>
    {
        options.Intents = GatewayIntents.All;
        options.Presence = new PresenceProperties(UserStatusType.Online);
    })
    .AddDiscordRest()
    .AddApplicationCommands()
    .AddOpenApi()
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

app.UseHttpsRedirection();
app.MapControllers();
app.Run();