using MaelstromNode.Interfaces;
using MaelstromNode.Models;
using MaelstromNode.Models.MessageBodies;

namespace MaelstromNode.Workloads;

internal class UniqueIdGenerator(ILogger<UniqueIdGenerator> logger, IReceiver receiver, ISender sender) : MaelstromNode(logger, receiver, sender)
{
    protected new ILogger<UniqueIdGenerator> logger = logger;
    private int _idCounter = 0;

    [MaelstromHandler(Generate.GenerateType)]
    public async Task HandleGenerate(Message message)
    {
        message.DeserializeAs<Generate>();
        var generatedId = GenerateId();
        logger.LogInformation("Received Generate request, generated id {Id}", generatedId);
        await ReplyAsync(message, new GenerateOk(GenerateId()));
    }

    private int GenerateId()
    {
        // Id is generated from a combination of process Id, random seed and message Id.
        return (1000000000 * _idCounter++) + (100000 * (new Random()).Next(0, 100)) + Environment.ProcessId;
    }
}
