using EchoService;
using Maelstrom;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<EchoServer>();
MaelstromNodeBuilder.SetupDependencies(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
