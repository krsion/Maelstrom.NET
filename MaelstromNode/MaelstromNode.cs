using MaelstromNode.Interfaces;
using MaelstromNode.Models;
using MaelstromNode.Models.MessageBodies;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace MaelstromNode;

internal class MaelstromNode(IReceiver receiver, ISender sender) : BackgroundService
{
    private readonly IReceiver _receiver = receiver;
    private readonly ISender _sender = sender;
    public string NodeId = "";
    public string[] NodeIds = Array.Empty<string>();
    private int _msgId = 0;
    private Dictionary<string, Func<MessageBody, Task>> _message_handlers = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitAsync();
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await RecvAsync();
            if (message != null)
            {
                Console.WriteLine($"Received message of type: {message.Body.Type}");
                if (_message_handlers.TryGetValue(message.Body.Type, out var handler))
                {
                    await handler(message.Body);
                }
                else
                {
                    await ErrorAsync(message, ErrorCodes.NotSupported, $"Message type {message.Body.Type} not supported");
                }
            }
        }
    }

    private async Task InitAsync()
    {
        var message = await RecvAsync();
        if (message == null ||  message.Body == null)
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
        Console.WriteLine($"Node initialized. Node ID: {NodeId}");
        await ReplyAsync(message, new InitOk());
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _sender.SendAsync("Goodbye!");
        await base.StopAsync(cancellationToken);
    }

    private async Task<Message?> RecvAsync()
    {
        var rawMessage = await _receiver.RecvAsync();
        if (rawMessage == null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<Message>(rawMessage);
        }
        catch (JsonException e)
        {
            Console.WriteLine($"Error deserializing message: {e.Message}");
            return null;
        }
    }

    public async Task SendAsync(string destination, MessageBody body)
    {
        body.MsgId = _msgId;
        var message = new Message(NodeId, destination, body);
        await _sender.SendAsync(message.Serialize());
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
