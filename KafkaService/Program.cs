using KafkaService;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<KafkaLog>();
MaelstromNode.MaelstromNode.SetupDependencies(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
