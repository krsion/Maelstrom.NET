using Maelstrom.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Maelstrom;

public static class MaelstromNodeBuilder
{
    public static IServiceCollection AddMaelstromNodeWorkload<TWorkload, TRec, TSend>(this IServiceCollection services)
        where TWorkload : Workload
        where TRec : class, IReceiver
        where TSend : class, ISender
        => services
            .SetupMaelstromNodeDependencies<TRec, TSend>()
            .AddHostedService<TWorkload>();

    public static IServiceCollection AddMaelstromNodeWorkload<TWorkload>(this IServiceCollection services)
        where TWorkload : Workload
        => services.AddMaelstromNodeWorkload<TWorkload, StdinReceiver, StdoutSender>();

    public static IServiceCollection SetupMaelstromNodeDependencies<TRec, TSend>(this IServiceCollection services)
        where TRec : class, IReceiver
        where TSend : class, ISender
        => services
            .AddSingleton<IReceiver, TRec>()
            .AddSingleton<ISender, TSend>()
            .AddSingleton<IMaelstromNode, MaelstromNode>();

    public static IServiceCollection SetupMaelstromNodeDependencies(this IServiceCollection services)
        => services.SetupMaelstromNodeDependencies<StdinReceiver, StdoutSender>();
}