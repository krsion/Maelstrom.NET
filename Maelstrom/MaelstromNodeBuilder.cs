using Maelstrom.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Maelstrom;

public static class MaelstromNodeBuilder
{
    public static IServiceCollection AddMaelstromNodeWorkload<TWorkload, TRec, TSend>(IServiceCollection services)
        where TWorkload : Workload
        where TRec : class, IReceiver
        where TSend : class, ISender
    {
        services = SetupDependencies<TRec, TSend>(services);
        services.AddHostedService<TWorkload>();
        return services;
    }

    public static IServiceCollection AddMaelstromNodeWorkload<TWorkload>(IServiceCollection services)
        where TWorkload : Workload
        => AddMaelstromNodeWorkload<TWorkload, StdinReceiver, StdoutSender>(services);

    public static IServiceCollection SetupDependencies<TRec, TSend>(IServiceCollection services)
        where TRec : class, IReceiver
        where TSend : class, ISender
    {
        services.AddSingleton<IReceiver, TRec>();
        services.AddSingleton<ISender, TSend>();
        services.AddSingleton<IMaelstromNode, MaelstromNode>();
        return services;
    }

    public static IServiceCollection SetupDependencies(IServiceCollection services) => SetupDependencies<StdinReceiver, StdoutSender>(services);
}