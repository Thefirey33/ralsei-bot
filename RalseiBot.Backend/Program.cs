// Ralsei Bot Discord API 
// This is the side of the bot that communicates with the Discord API.

using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services
    .AddDiscordGateway(options =>
    {
        options.Intents = GatewayIntents.All;
        options.Presence = new PresenceProperties(UserStatusType.Online);
    })
    .AddApplicationCommands()
    .AddOpenApi()
    .AddEndpointsApiExplorer()
    .AddControllers();

var app = builder.Build();

app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(10) });

app.MapDefaultEndpoints();
app.AddModules(typeof(Program).Assembly);

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
