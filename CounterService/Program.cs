using CounterService;
using Maelstrom;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Counter>();
MaelstromNodeBuilder.SetupDependencies(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
