using UniqueIdService;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<UniqueIdGenerator>();
MaelstromNode.MaelstromNode.SetupDependencies(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
