using EchoService;
using Maelstrom;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
MaelstromNodeBuilder.AddMaelstromNodeWorkload<EchoServer>(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
