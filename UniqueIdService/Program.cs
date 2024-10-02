using Maelstrom;
using UniqueIdService;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<UniqueIdGenerator>();
MaelstromNodeBuilder.SetupDependencies(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
