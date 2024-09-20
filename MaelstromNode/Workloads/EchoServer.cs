using MaelstromNode.Interfaces;
using MaelstromNode.Models;
using MaelstromNode.Models.MessageBodies;

namespace MaelstromNode.Workloads;

internal class EchoServer(IReceiver receiver, ISender sender) : MaelstromNode(receiver, sender)
{
    [MaelstromHandler(Echo.EchoType)]
    public async Task HandleEcho(Message message)
    {
        message.DeserializeAs<Echo>();
        var echo = (Echo)message.Body;
        Console.WriteLine($"Echoing message: {echo.EchoMessage}");
        await ReplyAsync(message, new EchoOk(echo.EchoMessage));
    }
}
