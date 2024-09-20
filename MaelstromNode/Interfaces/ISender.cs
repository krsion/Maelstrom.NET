namespace MaelstromNode.Interfaces;

internal interface ISender
{
    Task SendAsync(string message);
}
