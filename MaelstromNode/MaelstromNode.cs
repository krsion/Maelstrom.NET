using MaelstromNode.Interfaces;
using MaelstromNode.Models;
using MaelstromNode.Models.MessageBodies;
using System.Reflection;
using System.Text.Json;

namespace MaelstromNode;

internal class MaelstromNode(ILogger<MaelstromNode> logger, IReceiver receiver, ISender sender) : BackgroundService
{
    protected readonly ILogger logger = logger;
    private readonly IReceiver _receiver = receiver;
    private readonly ISender _sender = sender;
    public string NodeId = "";
    public string[] NodeIds = [];
    private int _msgId = 0;
    private Dictionary<string, Func<Message, Task>> MessageHandlers => GetType()
            .GetMethods()
            .Where(m => m.GetCustomAttributes().OfType<MaelstromHandlerAttribute>().Any())
            .ToDictionary(m => m.GetCustomAttribute<MaelstromHandlerAttribute>()!.MessageType, m => (Func<Message, Task>)m.CreateDelegate(typeof(Func<Message, Task>), this));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting...");
        await InitAsync();
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await RecvAsync();
            if (message != null)
            {
                logger.LogInformation("Received message of type: {MessageType}", message.Body.Type);
                if (MessageHandlers.TryGetValue(message.Body.Type, out var handler))
                {
                    await handler(message);
                }
                else
                {
                    logger.LogError("Message type {MessageType} not supported", message.Body.Type);
                    await ErrorAsync(message, ErrorCodes.NotSupported, $"Message type {message.Body.Type} not supported");
                }
            }
        }
    }

    private async Task InitAsync()
    {
        logger.LogInformation("Awaiting init message");
        var message = await RecvAsync();
        if (message == null || message.Body == null)
        {
            throw new Exception("Failed to receive init message");
        }
        if (message.Body.Type != "init")
        {
            await ErrorAsync(message, ErrorCodes.MalformedRequest, "First message must be an init message");
            throw new Exception("First message must be an init message");
        }
        message.DeserializeAs<Init>();
        NodeId = ((Init)message.Body).NodeId;
        NodeIds = ((Init)message.Body).NodeIds;
        logger.LogInformation("Node initialized. Node ID: {NodeId}", NodeId);
        await ReplyAsync(message, new InitOk());
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Stopping...");
        await base.StopAsync(cancellationToken);
    }

    private async Task<Message?> RecvAsync()
    {
        var rawMessage = await _receiver.RecvAsync();
        logger.LogDebug("Received message: {RawMessage}", rawMessage);
        if (rawMessage == null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Message>(rawMessage);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Error deserializing message");
            return null;
        }
    }

    public async Task SendAsync(string destination, MessageBody body)
    {
        body.MsgId = _msgId;
        var message = new Message(NodeId, destination, body);
        var rawMessage = message.Serialize();
        logger.LogDebug("Sending message: {RawMessage}", rawMessage);
        await _sender.SendAsync(rawMessage);
        _msgId++;
    }

    public async Task ReplyAsync(Message originalMessage, MessageBody body)
    {
        body.InReplyTo = (int)originalMessage.Body.MsgId!;
        await SendAsync(originalMessage.Src, body);
    }

    public async Task ErrorAsync(Message originalMessage, ErrorCodes errorCode, string errorMessage)
    {
        var body = new ErrorBody(errorCode, errorMessage);
        await ReplyAsync(originalMessage, body);
    }
}
