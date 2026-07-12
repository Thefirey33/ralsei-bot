using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
builder.Services
    .AddServiceDiscoveryCore()
    .AddDnsServiceEndpointProvider();

var backendService
    = builder.AddProject<Projects.RalseiBot_Backend>("ralseibotbackend");

builder.AddProject<Projects.RalseiBot_Web>("ralseibotweb")
    .WithReference(backendService);

builder.Build().Run();