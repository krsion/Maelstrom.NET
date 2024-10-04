using Maelstrom;
using UniqueIdService;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
MaelstromNodeBuilder.AddMaelstromNodeWorkload<UniqueIdGenerator>(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
