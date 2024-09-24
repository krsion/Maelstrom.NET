using MaelstromNode.Interfaces;
using MaelstromNode.Models;
using MaelstromNode.Models.MessageBodies;
using System.Text.Json;

namespace MaelstromNode.Workloads;

internal class BroadcastService(ILogger<BroadcastService> logger, IReceiver receiver, ISender sender) : MaelstromNode(logger, receiver, sender)
{
    protected new ILogger<BroadcastService> logger = logger;
    private HashSet<int> _broadcastMessages = [];
    private Dictionary<string, string[]> _topology = [];

    [MaelstromHandler(Broadcast.BroadcastType)]
    public async Task HandleBroadcast(Message message)
    {
        message.DeserializeAs<Broadcast>();
        var broadcastMessage = ((Broadcast)message.Body).BroadcastMessage;
        logger.LogInformation("Received broadcast message: {BroadcastMessage}", broadcastMessage);
        _broadcastMessages.Add(broadcastMessage);
        await ReplyAsync(message, new BroadcastOk());
    }

    [MaelstromHandler(Read.ReadType)]
    public async Task HandleRead(Message message)
    {
        message.DeserializeAs<Read>();
        logger.LogInformation("Received read request");
        await ReplyAsync(message, new ReadOk(_broadcastMessages.ToArray()));
    }

    [MaelstromHandler(Topology.TopologyType)]
    public async Task HandleTopology(Message message)
    {
        message.DeserializeAs<Topology>();
        var topologyMessage = (Topology)message.Body;
        logger.LogInformation("Received topology: {topology}", topologyMessage.TopologyData);
        var topology = topologyMessage.TopologyData.Deserialize<Dictionary<string, string[]>>();
        if (topology == null)
        {
            await ErrorAsync(message, ErrorCodes.MalformedRequest, "Malformed topology data");
            throw new Exception($"Malformed topology data: {topologyMessage.TopologyData}");
        }
        _topology = topology;
        await ReplyAsync(message, new TopologyOk());
    }
}
