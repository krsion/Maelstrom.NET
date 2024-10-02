HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<BroadcastService.BroadcastService>();
MaelstromNode.MaelstromNode.SetupDependencies(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
