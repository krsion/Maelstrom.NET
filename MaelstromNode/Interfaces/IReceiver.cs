namespace MaelstromNode.Interfaces;

internal interface IReceiver
{
    Task<string?> RecvAsync();
}
