// Ralsei Bot Discord API 
// This is the side of the bot that communicates with the Discord API.

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Rest;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add logging so we can log what the program is doing.

builder.Services
    .AddLogging()
    .AddAntiforgery();

builder.Logging
    .ClearProviders()
    .AddConsole();
// Register the MySQL databases, so the bot can use them properly.
builder.AddKeyedMySqlDataSource("ScoreDB");
builder.AddKeyedMySqlDataSource("TrustDB");
builder.AddKeyedMySqlDataSource("ServerDB");

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
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.UseHsts();
app.MapControllers();
app.Run();