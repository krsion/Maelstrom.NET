namespace MaelstromNode.Interfaces;

public interface IReceiver : IDisposable
{
    Task<string?> RecvAsync();

    Task<string?> RecvAsync(CancellationToken cancellationToken);
}
