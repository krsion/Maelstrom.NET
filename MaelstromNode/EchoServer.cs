using EchoService.Models.MessageBodies;
using MaelstromNode;
using MaelstromNode.Interfaces;
using MaelstromNode.Models;

namespace EchoService;

internal class EchoServer(ILogger<EchoServer> logger, IReceiver receiver, ISender sender) : MaelstromNode.MaelstromNode(logger, receiver, sender)
{
    protected new ILogger<EchoServer> logger = logger;

    [MaelstromHandler(Echo.EchoType)]
    public async Task HandleEcho(Message message)
    {
        message.DeserializeAs<Echo>();
        var echo = (Echo)message.Body;
        logger.LogInformation("Echoing message: {EchoMessage}", echo.EchoMessage);
        await ReplyAsync(message, new EchoOk(echo.EchoMessage));
    }
}
