using Maelstrom.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Maelstrom;

public static class MaelstromNodeBuilder
{

    public static IServiceCollection SetupDependencies<RecT, SendT>(IServiceCollection services)
        where RecT : class, IReceiver
        where SendT : class, ISender
    {
        services.AddSingleton<IReceiver, RecT>();
        services.AddSingleton<ISender, SendT>();
        services.AddSingleton<IMaelstromNode, MaelstromNode>();
        return services;
    }

    public static IServiceCollection SetupDependencies(IServiceCollection services) => SetupDependencies<StdinReceiver, StdoutSender>(services);
}