using MaelstromNode.Interfaces;

namespace MaelstromNode;

internal class StdinReceiver : IReceiver
{
    private readonly StreamReader _stream;

    public StdinReceiver() {
        var inputStream = Console.OpenStandardInput();
        _stream = new StreamReader(inputStream);
    }

    public async Task<string?> RecvAsync()
    {
        return await _stream.ReadLineAsync();
    }
}
