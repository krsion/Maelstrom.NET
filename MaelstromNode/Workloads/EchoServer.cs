using MaelstromNode.Interfaces;
using MaelstromNode.Models;
using MaelstromNode.Models.MessageBodies;
using Microsoft.Extensions.Logging;

namespace MaelstromNode.Workloads;

internal class EchoServer(ILogger<EchoServer> logger, IReceiver receiver, ISender sender) : MaelstromNode(logger, receiver, sender)
{
    [MaelstromHandler(Echo.EchoType)]
    public async Task HandleEcho(Message message)
    {
        message.DeserializeAs<Echo>();
        var echo = (Echo)message.Body;
        logger.LogInformation("Echoing message: {EchoMessage}", echo.EchoMessage);
        await ReplyAsync(message, new EchoOk(echo.EchoMessage));
    }
}
