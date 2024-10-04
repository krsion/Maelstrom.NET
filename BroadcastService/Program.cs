using Maelstrom;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
MaelstromNodeBuilder.AddMaelstromNodeWorkload<BroadcastService.BroadcastService>(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
