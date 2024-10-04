using Maelstrom.Interfaces;

namespace Maelstrom;

internal class StdoutSender : ISender
{
    private readonly StreamWriter _stream;
    public StdoutSender()
    {
        var outputStream = Console.OpenStandardOutput();
        _stream = new StreamWriter(outputStream)
        {
            AutoFlush = true
        };
        Console.SetOut(_stream);
    }
    public async Task SendAsync(string message) => await _stream.WriteLineAsync(message);

    public void Dispose() => _stream.Dispose();
}
