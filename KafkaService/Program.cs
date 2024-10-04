using KafkaService;
using Maelstrom;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
MaelstromNodeBuilder.AddMaelstromNodeWorkload<KafkaLog>(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
