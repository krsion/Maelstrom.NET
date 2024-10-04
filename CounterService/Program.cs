using CounterService;
using Maelstrom;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
MaelstromNodeBuilder.AddMaelstromNodeWorkload<Counter>(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
