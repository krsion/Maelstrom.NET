namespace MaelstromNode.Interfaces;

internal interface ISender : IDisposable
{
    Task SendAsync(string message);
}
