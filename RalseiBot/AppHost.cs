using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var compose = builder.AddDockerComposeEnvironment("env");

builder.Services
    .AddServiceDiscoveryCore()
    .AddDnsServiceEndpointProvider();

var mySql = builder
    .AddMySql("mysql")
    .WithImageTag("8.0-debian")
    .WithDataVolume(isReadOnly: false)
    .WithPhpMyAdmin(phpAdmin =>
        phpAdmin.WithHostPort(3000))
    .WithLifetime(ContainerLifetime.Persistent);

// Databases for the Ralsei Bot.
// Each one of these databases depend on the MySQL database at the top, and each one with their own unique purpose.

// Close this region in your IDE to prevent an SQL-tastrophe.

#region Databases

// The Score DB. This is the high score that the user will have. 
var scoredb = mySql.AddDatabase("scoredb");
// Trusted User Database.
// Each trusted user is put into this database and the bot will NOT kick them for being too new.
var trustedUser = mySql.AddDatabase("trustdb");

// Server Database.
// This is where the bot configures itself for each server.
var serverdb = mySql.AddDatabase("serverdb");

var warningb = mySql.AddDatabase("warningdb");

#endregion

// The Backend for the Ralsei Bot.
// Each of the databases will connect to this project, where it will manage it.

var filteringService = builder.AddUvicornApp(
        "ralseibotclassification",
        "../RalseiBot.Classification",
        "main:app")
    .WithHttpEndpoint(5000, 5000, env: "PORT");

var backendService
    = builder.AddProject<RalseiBot_Backend>("ralseibotbackend")
        .WithHttpEndpoint(8080, 8080)
        .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8080")
        .WaitFor(filteringService)
        .WithReference(filteringService)
        .WaitFor(mySql)
        .WaitFor(scoredb)
        .WaitFor(trustedUser)
        .WaitFor(warningb)
        .WaitFor(serverdb)
        .WithReference(scoredb)
        .WithReference(trustedUser)
        .WithReference(serverdb)
        .WithReference(warningb);


builder.AddProject<RalseiBot_Web>("ralseibotfrontend")
    .WithReference(backendService)
    .WithExternalHttpEndpoints()
    .WithEnvironment("ASPNETCORE_HTTP_PORTS", "8000")
    .WithHttpEndpoint(8000, 8000, isProxied: false)
    .WaitFor(backendService);

builder.Build().Run();