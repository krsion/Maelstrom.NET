using Maelstrom.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Maelstrom;

public class Workload : BackgroundService
{
    private readonly ILogger<Workload> logger;
    protected readonly IMaelstromNode node;

    public Workload(ILogger<Workload> logger, IMaelstromNode node)
    {
        this.logger = logger;
        this.node = node;
        this.node.AddMessageHandlers(this);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting...");
        await node.RunAsync(stoppingToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping...");
        node.Dispose();
        await base.StopAsync(cancellationToken);
    }
}
