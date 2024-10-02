using EchoService;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<EchoServer>();
MaelstromNode.MaelstromNode.SetupDependencies(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
