using Maelstrom.Models;

namespace Maelstrom.Interfaces;

public interface IMaelstromNode : IDisposable
{
    string NodeId { get; }
    string[] NodeIds { get; }
    IKvStoreClient SeqKvStoreClient { get; }
    IKvStoreClient LinKvStoreClient { get; }
    void AddMessageHandlers<T>(T workload) where T : class;
    Task ErrorAsync(Message originalMessage, ErrorCodes errorCode, string errorMessage);
    Task ReplyAsync(Message originalMessage, MessageBody body);
    Task<Message> RpcAsync(string destination, MessageBody body);
    Task RunAsync(CancellationToken stoppingToken);
    Task SendAsync(string destination, MessageBody body);
}