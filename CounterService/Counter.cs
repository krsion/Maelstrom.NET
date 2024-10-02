using CounterService.Models.MessageBodies;
using MaelstromNode;
using MaelstromNode.Interfaces;
using MaelstromNode.Models;

namespace CounterService;

internal class Counter(ILogger<Counter> logger, IReceiver receiver, ISender sender) : MaelstromNode.MaelstromNode(logger, receiver, sender)
{
    private const string _counterKey = "counter";
    private const int _maxAttempts = 10;
    protected new ILogger<Counter> logger = logger;

    [MaelstromHandler(Read.ReadType)]
    public async Task HandleRead(Message message)
    {
        logger.LogDebug("Received counter read");

        // Increment by 0 to force read of latest value from store.
        var latestValue = await IncrementValue(0);
        logger.LogInformation("Counter read OK, value {value}", latestValue);
        await ReplyAsync(message, new ReadOk<int>(latestValue));
    }

    [MaelstromHandler(Add.AddType)]
    public async Task HandleAdd(Message message)
    {
        message.DeserializeAs<Add>();
        var add = (Add)message.Body;
        logger.LogDebug("Received counter add {delta}", add.Delta);
        await ReplyAsync(message, new AddOk());
        var latestValue = await IncrementValue(add.Delta);
        logger.LogInformation("Counter incremented by {delta} to {val}", add.Delta, latestValue);
    }

    private async Task<int> GetValue()
    {
        try
        {
            return await SeqKvStoreClient.ReadAsync<string, int>(_counterKey);
        }
        catch (KvStoreKeyNotFoundException)
        {
            logger.LogInformation("Counter does not exist, assume 0");
            return 0;
        }
    }

    private async Task<int> IncrementValue(int delta)
    {
        int attempts = 1;
        while (attempts <= _maxAttempts)
        {
            logger.LogDebug("Increment counter by {delta}, attempt {attempts}", delta, attempts);
            var latestValue = await GetValue();
            try
            {
                await SeqKvStoreClient.CasAsync(_counterKey, latestValue, latestValue + delta, createIfNotExists: true);
            }
            catch (KvStoreCasPreconditionFailed)
            {
                logger.LogWarning("CAS failed, waiting and retrying");
                await Task.Delay(10 + new Random().Next(-2, 2));
                attempts++;
                continue;
            }

            logger.LogDebug("Increment succeeded");
            return latestValue + delta;
        }

        logger.LogError("Increment failed after {attempts} attempts", _maxAttempts);
        throw new Exception("Increment failed after max attempts");
    }
}
