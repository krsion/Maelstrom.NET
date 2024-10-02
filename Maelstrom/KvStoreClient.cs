using Maelstrom.Interfaces;
using Maelstrom.Models;
using Maelstrom.Models.MessageBodies;
using Maelstrom.Models.MessageBodies.KvStore;
using Microsoft.Extensions.Logging;

namespace Maelstrom;

public class KvStoreClient(IMaelstromNode node, ILogger<IMaelstromNode> logger, string serviceName)
{
    private readonly string _serviceName = serviceName;
    private readonly ILogger<IMaelstromNode> logger = logger;
    private readonly IMaelstromNode _node = node;

    public async Task<U> ReadAsync<T, U>(T key)
    {
        logger.LogDebug("Reading key {key}", key);
        Read<T> read = new(key);
        var response = await _node.RpcAsync(_serviceName, read);
        if (response.Body.Type == ErrorBody.ErrorBodyType)
        {
            response.DeserializeAs<ErrorBody>();
            var error = (ErrorBody)response.Body;
            logger.LogDebug("Error reading key {key}: {errorCode} {errorText}", key, error.ErrorCode, error.ErrorText);
            if (error.ErrorCode == ErrorCodes.KeyDoesNotExist)
            {
                throw new KvStoreKeyNotFoundException($"Key {key} does not exist");
            }

            throw new KvStoreException($"Error reading key {key}: {error.ErrorText}");
        }

        response.DeserializeAs<ReadOk<U>>();
        var readOk = (ReadOk<U>)response.Body;
        logger.LogDebug("Read key {key}: {value}", key, readOk.Value);
        return readOk.Value;
    }

    public async Task WriteAsync<T, U>(T key, U value)
    {
        logger.LogDebug("Writing key {key}: {value}", key, value);
        Write<T, U> write = new(key, value);
        var response = await _node.RpcAsync(_serviceName, write);
        switch (response.Body.Type)
        {
            case ErrorBody.ErrorBodyType:
                response.DeserializeAs<ErrorBody>();
                var error = (ErrorBody)response.Body;
                throw new KvStoreException($"Error writing key {key}: {error.ErrorText}");

            case WriteOk.WriteOkType:
                logger.LogDebug("Wrote key {key}: {value}", key, value);
                break;

            default:
                throw new Exception($"Unexpected return type for key write: {response.Body.Type}");
        }
    }

    public async Task CasAsync<T, U>(T key, U from, U to, bool createIfNotExists = false)
    {
        logger.LogDebug("CAS key {key} from {from} to {to}", key, from, to);
        Cas<T, U> cas = new(key, from, to, createIfNotExists);
        var response = await _node.RpcAsync(_serviceName, cas);
        switch (response.Body.Type)
        {
            case ErrorBody.ErrorBodyType:
                response.DeserializeAs<ErrorBody>();
                var error = (ErrorBody)response.Body;
                logger.LogDebug("Error setting key {key}: {errorCode} {errorText}", key, error.ErrorCode, error.ErrorText);

                throw error.ErrorCode switch
                {
                    ErrorCodes.KeyDoesNotExist => new KvStoreKeyNotFoundException($"Key {key} does not exist"),
                    ErrorCodes.PreconditionFailed => new KvStoreCasPreconditionFailed($"CAS precondition failed for key {key}"),
                    _ => new KvStoreException($"Error setting key {key}: {error.ErrorText}"),
                };
            case CasOk.CasOkType:
                logger.LogDebug("CAS key {key} from {from} to {to} succeeded", key, from, to);
                break;

            default:
                throw new Exception($"Unexpected return type for CAS operation: {response.Body.Type}");
        }
    }
}

public class KvStoreException(string message) : Exception(message)
{
}

public class KvStoreKeyNotFoundException(string message) : KvStoreException(message)
{
}

public class KvStoreCasPreconditionFailed(string message) : KvStoreException(message)
{
}