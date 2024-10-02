namespace MaelstromNode.Interfaces;

public interface ISender : IDisposable
{
    Task SendAsync(string message);
}
