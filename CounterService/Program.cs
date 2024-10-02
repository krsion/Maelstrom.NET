using CounterService;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Counter>();
MaelstromNode.MaelstromNode.SetupDependencies(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
