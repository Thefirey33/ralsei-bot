using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);
builder.Services
    .AddServiceDiscoveryCore()
    .AddDnsServiceEndpointProvider();

var mySql = builder
    .AddMySql("MySQLDatabase")
    .WithDataVolume(isReadOnly: false)
    .WithPhpMyAdmin()
    .WithLifetime(ContainerLifetime.Persistent);

// Databases for the Ralsei Bot.
// Each one of these databases depend on the MySQL database at the top, and each one with their own unique purpose.

// Close this region in your IDE to prevent an SQL-tastrophe.

#region Databases

// The Score DB. This is the high score that the user will have. 
var scoreDb =
    mySql
        .AddDatabase("ScoreDB")
        .WithCreationScript("""
                            CREATE DATABASE IF NOT EXISTS ScoreDB;
                            CREATE TABLE IF NOT EXISTS ScoreDB.users (
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
var serverDb
    = mySql.AddDatabase("ServerDB")
        .WithCreationScript("""
                            CREATE DATABASE IF NOT EXISTS ServerDB;
                            CREATE TABLE IF NOT EXISTS ServerDB.servers (
                                id INT AUTO_INCREMENT PRIMARY KEY,
                                ralsei_channel_id BIGINT NOT NULL,
                                general_channel_id BIGINT NOT NULL,
                                moderation_channel_id BIGINT NOT NULL
                            );
                            """
        );

#endregion

// The Backend for the Ralsei Bot.
// Each of the databases will connect to this project, where it will manage it.

var backendService
    = builder.AddProject<RalseiBot_Backend>("RalseiBotBackend")
        .WithHttpEndpoint(8080)
        .WithReference(scoreDb) // Reference each
        .WithReference(trustedUser) // Database, so we can
        .WithReference(serverDb) // Use it.
        .WaitFor(scoreDb) // Wait for each
        .WaitFor(trustedUser) // Database
        .WaitFor(serverDb); // To Initialize.


builder.AddProject<RalseiBot_Web>("RalseiBotFrontend")
    .WithReference(backendService)
    .WithExternalHttpEndpoints()
    .WithHttpEndpoint(8000)
    .WaitFor(backendService);

builder.Build().Run();