using Maelstrom;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<BroadcastService.BroadcastService>();
MaelstromNodeBuilder.SetupDependencies(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
