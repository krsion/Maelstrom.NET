using Maelstrom;
using Maelstrom.Interfaces;
using Maelstrom.Models;
using TransationRwRegisterService.Models;
using TransationRwRegisterService.Models.MessageBodies;

namespace TransationRwRegisterService;

internal class TransactionRwRegister(ILogger<TransactionRwRegister> logger, IMaelstromNode node) : Workload(logger, node)
{
    private readonly ILogger<TransactionRwRegister> logger = logger;
    private readonly Dictionary<int, int> _store = [];
    private readonly SemaphoreSlim _storeLock = new(1);

    [MaelstromHandler(Transaction.TxnType)]
    public async Task HandleTransaction(Message message)
    {
        message.DeserializeAs<Transaction>();
        var transaction = (Transaction)message.Body;
        var completedTransactions = await ExecuteTransactions(transaction.Operations);
        await node.ReplyAsync(message, new TransactionOk(completedTransactions));
    }

    private async Task<List<Operation>> ExecuteTransactions(List<Operation> operations)
    {
        List<Operation> completedTransactions = [];
        await _storeLock.WaitAsync();
        try
        {
            foreach (var operation in operations)
            {
                var completedOperation = operation.OperationType switch
                {
                    OperationType.Read => ExecuteRead(operation),
                    OperationType.Write => ExecuteWrite(operation),
                    _ => throw new NotImplementedException(),
                };
                completedTransactions.Add(completedOperation);
            }
        }
        finally
        {
            _storeLock.Release();
        }

        return completedTransactions;
    }

    private Operation ExecuteWrite(Operation operation)
    {
        logger.LogDebug("WRITE: {k} = {v}", operation.Key, operation.Val);
        if (operation.Val == null)
        {
            throw new Exception($"Cannot write null value to key {operation.Key}");
        }

        _store[operation.Key] = (int)operation.Val;
        return operation;
    }

    private Operation ExecuteRead(Operation operation)
    {
        _store.TryGetValue(operation.Key, out int val);
        operation.Val = val;
        logger.LogDebug("READ:  {k} = {v}", operation.Key, operation.Val);
        return operation;
    }
}
