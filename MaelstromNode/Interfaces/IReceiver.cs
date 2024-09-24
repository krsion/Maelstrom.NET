namespace MaelstromNode.Interfaces;

internal interface IReceiver : IDisposable
{
    Task<string?> RecvAsync();

    Task<string?> RecvAsync(CancellationToken cancellationToken);
}
