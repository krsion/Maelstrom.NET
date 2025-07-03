using CounterService;
using Maelstrom;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace; // All logs go to stderr
        });
    })
    .ConfigureServices(services =>
    {
        services.AddMaelstromNodeWorkload<CounterService.CounterService>();
    });

using IHost host = builder.Build();
await host.RunAsync();
