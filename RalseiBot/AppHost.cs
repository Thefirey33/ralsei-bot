using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var compose = builder.AddDockerComposeEnvironment("RalseiDockerCompose");

builder.Services
    .AddServiceDiscoveryCore()
    .AddDnsServiceEndpointProvider();

var mySql = builder
    .AddMySql("mysql")
    .WithImageTag("8.0-debian")
    .WithDataVolume(isReadOnly: false)
    .WithPhpMyAdmin(phpAdmin => phpAdmin.WithHostPort(3000))
    .WithLifetime(ContainerLifetime.Persistent);

// Databases for the Ralsei Bot.
// Each one of these databases depend on the MySQL database at the top, and each one with their own unique purpose.

// Close this region in your IDE to prevent an SQL-tastrophe.

#region Databases

// The Score DB. This is the high score that the user will have. 
var scoredb =
    mySql
        .AddDatabase("scoredb")
        .WithCreationScript("""
                            CREATE DATABASE IF NOT EXISTS scoredb;
                            CREATE TABLE IF NOT EXISTS scoredb.users (
                                id INT AUTO_INCREMENT PRIMARY KEY,
                                user_id BIGINT NOT NULL,
                                username VARCHAR(255) NOT NULL,
                                score INT NOT NULL
                            );
                            """
        );

// Trusted User Database.
// Each trusted user is put into this database and the bot will NOT kick them for being too new.
var trustedUser =
    mySql.AddDatabase("TrustDB")
        .WithCreationScript("""
                            CREATE DATABASE IF NOT EXISTS TrustDB;
                            CREATE TABLE IF NOT EXISTS TrustDB.users (
                                id INT AUTO_INCREMENT PRIMARY KEY,
                                user_id BIGINT NOT NULL
                            );
                            """
        );

// Server Database.
// This is where the bot configures itself for each server.
var serverdb
    = mySql.AddDatabase("serverdb")
        .WithCreationScript("""
                            CREATE DATABASE IF NOT EXISTS serverdb;
                            CREATE TABLE IF NOT EXISTS serverdb.servers (
                                id INT AUTO_INCREMENT PRIMARY KEY,
                                guild_id BIGINT NOT NULL,
                                ralsei_channel_id BIGINT,
                                general_channel_id BIGINT,
                                moderation_channel_id BIGINT,
                                UNIQUE (guild_id)
                            );
                            """
        );

var warningb = mySql.AddDatabase("warningdb")
    .WithCreationScript("""
                        CREATE DATABASE IF NOT EXISTS warningdb;
                        CREATE TABLE IF NOT EXISTS warningdb.users (
                            id INT AUTO_INCREMENT PRIMARY KEY,
                            user_id BIGINT NOT NULL,
                            warning_count INT NOT NULL,
                            UNIQUE (user_id)
                        );
                        """);

#endregion

// The Backend for the Ralsei Bot.
// Each of the databases will connect to this project, where it will manage it.

var filteringService = builder.AddUvicornApp(
        "ralseibotclassification",
        "../RalseiBot.Classification",
        "main:app")
    .WithHttpEndpoint(5000, env: "PORT");

var backendService
    = builder.AddProject<RalseiBot_Backend>("ralseibotbackend")
        .WithHttpEndpoint(8080)
        .WithReference(filteringService)
        .WaitFor(filteringService)
        .WaitFor(mySql) // Database Reference Section. Where the databases are referenced and processed.
        .WithReference(scoredb)
        .WithReference(trustedUser)
        .WithReference(serverdb)
        .WithReference(warningb)
        .WithComputeEnvironment(compose);


builder.AddProject<RalseiBot_Web>("ralseibotfrontend")
    .WithReference(backendService)
    .WithExternalHttpEndpoints()
    .WithHttpEndpoint(8000, isProxied: false)
    .WaitFor(backendService);

builder.Build().Run();