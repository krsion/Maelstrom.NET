using MaelstromNode.Interfaces;
using MaelstromNode.Models;
using MaelstromNode.Models.MessageBodies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace MaelstromNode;

public class MaelstromNode : BackgroundService, IMaelstromNode
{
    protected readonly ILogger<MaelstromNode> logger;
    private readonly IReceiver _receiver;
    private readonly ISender _sender;
    public string NodeId = "";
    public string[] NodeIds = [];
    protected readonly KvStoreClient SeqKvStoreClient;
    protected readonly KvStoreClient LinKvStoreClient;

    private int _msgId = 0;
    private readonly Dictionary<string, Func<Message, Task>> _messageHandlers;
    private readonly Dictionary<int, TaskCompletionSource<Message>> _replyHandlers = [];
    private readonly HashSet<Task> _activeHandlers = [];
    private readonly SemaphoreSlim _sendLock = new(1);
    private readonly SemaphoreSlim _replyHandlersLock = new(1);

    public MaelstromNode(ILogger<MaelstromNode> logger, IReceiver receiver, ISender sender)
    {
        this.logger = logger;
        _receiver = receiver;
        _sender = sender;
        _messageHandlers = GetType()
            .GetMethods()
            .Where(m => m.GetCustomAttributes().OfType<MaelstromHandlerAttribute>().Any())
            .ToDictionary(m => m.GetCustomAttribute<MaelstromHandlerAttribute>()!.MessageType, m => (Func<Message, Task>)m.CreateDelegate(typeof(Func<Message, Task>), this));
        SeqKvStoreClient = new(this, this.logger, "seq-kv");
        LinKvStoreClient = new(this, this.logger, "lin-kv");
    }

    public static IServiceCollection SetupDependencies<RecT, SendT>(IServiceCollection services)
        where RecT : class, IReceiver
        where SendT : class, ISender
    {
        services.AddSingleton<IReceiver, RecT>();
        services.AddSingleton<ISender, SendT>();
        return services;
    }

    public static IServiceCollection SetupDependencies(IServiceCollection services) => SetupDependencies<StdinReceiver, StdoutSender>(services);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting...");
        await InitAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            var message = await RecvAsync(stoppingToken);
            if (message != null)
            {
                await ProcessMessageAsync(message);
            }
            else
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        logger.LogInformation("Waiting for active tasks to complete...");
        await Task.WhenAll(_activeHandlers);
    }

    private async Task ProcessMessageAsync(Message message)
    {
        logger.LogInformation("Received message of type: {MessageType}", message.Body.Type);
        if (message.Body.InReplyTo != null)
        {
            int replyId = (int)message.Body.InReplyTo!;
            var replyTcs = await GetReplyHandler(replyId);
            if (replyTcs == null)
            {
                logger.LogError("No handler found for reply message with id {ReplyId}", replyId);
            }
            else
            {
                replyTcs.SetResult(message);
            }
        }
        else if (_messageHandlers.TryGetValue(message.Body.Type, out var handler))
        {
            var hTask = handler(message);
            _activeHandlers.Add(hTask);
        }
        else
        {
            logger.LogError("Message type {MessageType} not supported", message.Body.Type);
            await ErrorAsync(message, ErrorCodes.NotSupported, $"Message type {message.Body.Type} not supported");
        }
    }

    private async Task InitAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Awaiting init message");
        var message = await RecvAsync(cancellationToken);
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
        _sender.Dispose();
        _receiver.Dispose();
        await base.StopAsync(cancellationToken);
    }

    private async Task<Message?> RecvAsync(CancellationToken? cancellationToken = null)
    {
        string? rawMessage;
        if (cancellationToken != null)
        {
            rawMessage = await _receiver.RecvAsync(cancellationToken.Value);
        }
        else
        {
            rawMessage = await _receiver.RecvAsync();
        }

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
        await _sendLock.WaitAsync();
        try
        {
            body.MsgId = _msgId;
            var message = new Message(NodeId, destination, body);
            var rawMessage = message.Serialize();
            logger.LogDebug("Sending message: {RawMessage}", rawMessage);
            await _sender.SendAsync(rawMessage);
            _msgId++;
        }
        finally
        {
            _sendLock.Release();
        }
    }

    public async Task ReplyAsync(Message originalMessage, MessageBody body)
    {
        if (originalMessage.Body.MsgId == null)
        {
            throw new Exception("For reply, original message must have a MsgId");
        }
        body.InReplyTo = (int)originalMessage.Body.MsgId;
        await SendAsync(originalMessage.Src, body);
    }

    public async Task ErrorAsync(Message originalMessage, ErrorCodes errorCode, string errorMessage)
    {
        var body = new ErrorBody(errorCode, errorMessage);
        await ReplyAsync(originalMessage, body);
    }

    public async Task<Message> RpcAsync(string destination, MessageBody body)
    {
        Task<Message> replyTask;
        await _sendLock.WaitAsync();
        try
        {
            body.MsgId = _msgId;
            var message = new Message(NodeId, destination, body);
            var rawMessage = message.Serialize();
            replyTask = (await AddReplyHander(_msgId)).Task;
            logger.LogDebug("Sending RPC message: {RawMessage}", rawMessage);
            await _sender.SendAsync(rawMessage);
            _msgId++;
        }
        finally
        {
            _sendLock.Release();
        }

        return await replyTask;
    }

    private async Task<TaskCompletionSource<Message>> AddReplyHander(int msgId)
    {
        var tcs = new TaskCompletionSource<Message>();
        await _replyHandlersLock.WaitAsync();
        try
        {
            _replyHandlers.Add(msgId, tcs);
        }
        finally
        {
            _replyHandlersLock.Release();
        }

        return tcs;
    }

    private async Task<TaskCompletionSource<Message>?> GetReplyHandler(int msgId)
    {
        TaskCompletionSource<Message>? tcs;
        await _replyHandlersLock.WaitAsync();
        try
        {
            if (_replyHandlers.TryGetValue(msgId, out tcs))
            {
                _replyHandlers.Remove(msgId);
            }
        }
        finally
        {
            _replyHandlersLock.Release();
        }

        return tcs;
    }
}
