using MaelstromNode;
using MaelstromNode.Interfaces;
using MaelstromNode.Workloads;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<UniqueIdGenerator>();
builder.Services.AddSingleton<IReceiver, StdinReceiver>();
builder.Services.AddSingleton<ISender, StdoutSender>();

using IHost host = builder.Build();
await host.RunAsync();
