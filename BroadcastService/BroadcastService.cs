using BroadcastService.Models.MessageBodies;
using MaelstromNode;
using MaelstromNode.Interfaces;
using MaelstromNode.Models;
using MaelstromNode.Models.MessageBodies;
using System.Text.Json;

namespace BroadcastService;

internal class BroadcastService(ILogger<BroadcastService> logger, IReceiver receiver, ISender sender) : MaelstromNode.MaelstromNode(logger, receiver, sender)
{
    protected new ILogger<BroadcastService> logger = logger;
    private readonly HashSet<int> _broadcastMessages = [];
    private Dictionary<string, string[]> _topology = [];

    [MaelstromHandler(Broadcast.BroadcastType)]
    public async Task HandleBroadcast(Message message)
    {
        message.DeserializeAs<Broadcast>();
        var broadcastMessage = ((Broadcast)message.Body).BroadcastMessage;
        logger.LogInformation("Received broadcast message: {BroadcastMessage}", broadcastMessage);
        await ReplyAsync(message, new BroadcastOk());
        if (_broadcastMessages.Contains(broadcastMessage))
        {
            logger.LogInformation("Message already seen, ignoring broadcast");
        }
        else
        {
            var nextHops = GetNextHops(message);
            if (nextHops.Count > 0)
            {
                logger.LogInformation("Broadcasting message to next hops {nextHops}", nextHops);
                await Task.WhenAll(nextHops.Select(n => RpcAsync(n, new Broadcast(broadcastMessage))));
            }
            logger.LogInformation("Message broadcast successfully");
            _broadcastMessages.Add(broadcastMessage);
        }
    }

    [MaelstromHandler(Read.ReadType)]
    public async Task HandleRead(Message message)
    {
        message.DeserializeAs<Read>();
        logger.LogInformation("Received read request");
        await ReplyAsync(message, new ReadOk([.. _broadcastMessages]));
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

    private List<string> GetNextHops(Message message)
    {
        // Return neighbors excluding message source to avoid reflection.
        return _topology[NodeId].Where(n => n != message.Src).ToList();
    }
}
