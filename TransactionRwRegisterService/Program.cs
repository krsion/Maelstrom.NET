using Maelstrom;
using TransactionRwRegisterService;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
MaelstromNodeBuilder.AddMaelstromNodeWorkload<TransactionRwRegister>(builder.Services);

using IHost host = builder.Build();
await host.RunAsync();
