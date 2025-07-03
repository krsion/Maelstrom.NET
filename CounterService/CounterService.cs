using System.Data;
using CounterService.Models.MessageBodies;
using Maelstrom;
using Maelstrom.Interfaces;
using Maelstrom.Models;

namespace CounterService;

internal class CounterService(ILogger<CounterService> logger, IMaelstromNode node) : Workload(logger, node)
{
    private int positiveCounter = 0;
    private int negativeCounter = 0;

    private readonly ILogger<CounterService> logger = logger;

    [MaelstromHandler(Read.ReadType)]
    public async Task HandleRead(Message message)
    {
        int counter = positiveCounter - negativeCounter;
        logger.LogInformation("Counter read, value {value}", counter);
        await node.ReplyAsync(message, new ReadOk<int>(counter));
    }

    [MaelstromHandler(Add.AddType)]
    public async Task HandleAdd(Message message)
    {
        var add = message.DeserializeAs<Add>();

        logger.LogDebug("Received counter add {delta}", add.Delta);

        if (add.Delta < 0)
        {
            negativeCounter += -add.Delta;
        }
        else
        {
            positiveCounter += add.Delta;
        }

        var tasks = node.NodeIds
            .Where(x => x != node.NodeId)
            .Select(n => node.RpcAsync(n, new Merge(positiveCounter, negativeCounter)))
            .ToArray();

        await Task.WhenAll(tasks);

        await node.ReplyAsync(message, new AddOk());
        logger.LogInformation("Counter incremented by {delta} to {val}", add.Delta, positiveCounter - negativeCounter);
    }

    [MaelstromHandler(Merge.MergeType)]
    public async Task HandleMerge(Message message)
    {
        var merge = message.DeserializeAs<Merge>();
        logger.LogDebug("Received counter merge {value1}, {value2}", merge.PositiveCounter, merge.NegativeCounter);
        positiveCounter = Math.Max(positiveCounter, merge.PositiveCounter);
        negativeCounter = Math.Max(negativeCounter, merge.NegativeCounter);
        await node.ReplyAsync(message, new MergeOk(positiveCounter, negativeCounter));
        logger.LogInformation("Counter merged with {value1}, {value2} to {val1}, {val2}", merge.PositiveCounter, merge.NegativeCounter, positiveCounter, negativeCounter);
    }
}
