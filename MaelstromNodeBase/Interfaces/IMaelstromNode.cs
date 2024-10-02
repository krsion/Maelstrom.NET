using MaelstromNode.Models;

namespace MaelstromNode.Interfaces;

public interface IMaelstromNode
{
    Task ErrorAsync(Message originalMessage, ErrorCodes errorCode, string errorMessage);
    Task ReplyAsync(Message originalMessage, MessageBody body);
    Task<Message> RpcAsync(string destination, MessageBody body);
    Task SendAsync(string destination, MessageBody body);
}