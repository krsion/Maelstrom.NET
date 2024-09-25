using MaelstromNode.Models.MessageBodies;

namespace MaelstromNode;

internal class KvStoreClient(MaelstromNode node, ILogger<MaelstromNode> logger, string serviceName)
{
    private readonly string _serviceName = serviceName;
    private readonly ILogger<MaelstromNode> logger = logger;
    private readonly MaelstromNode _node = node;

    public async Task<U> ReadAsync<T, U>(T key)
    {
        logger.LogDebug("Reading key {key}", key);
        KvRead<T> read = new(key);
        var response = await _node.RpcAsync(_serviceName, read);
        if (response.Body.Type == ErrorBody.ErrorBodyType)
        {
            response.DeserializeAs<ErrorBody>();
            var error = (ErrorBody)response.Body;
            logger.LogDebug("Error reading key {key}: {errorCode} {errorText}", key, error.ErrorCode, error.ErrorText);
            if (error.ErrorCode == Models.ErrorCodes.KeyDoesNotExist)
            {
                throw new KvStoreKeyNotFoundException($"Key {key} does not exist");
            }

            throw new KvStoreException($"Error reading key {key}: {error.ErrorText}");
        }

        response.DeserializeAs<KvReadOk<U>>();
        var readOk = (KvReadOk<U>)response.Body;
        logger.LogDebug("Read key {key}: {value}", key, readOk.Value);
        return readOk.Value;
    }

    public async Task WriteAsync<T, U>(T key, U value)
    {
        logger.LogDebug("Writing key {key}: {value}", key, value);
        KvWrite<T, U> write = new(key, value);
        var response = await _node.RpcAsync(_serviceName, write);
        switch (response.Body.Type)
        {
            case ErrorBody.ErrorBodyType:
                response.DeserializeAs<ErrorBody>();
                var error = (ErrorBody)response.Body;
                throw new KvStoreException($"Error writing key {key}: {error.ErrorText}");

            case KvWriteOk.WriteOkType:
                logger.LogDebug("Wrote key {key}: {value}", key, value);
                break;

            default:
                throw new Exception($"Unexpected return type for key write: {response.Body.Type}");
        }
    }

    public async Task CasAsync<T, U>(T key, U from, U to)
    {
        logger.LogDebug("CAS key {key} from {from} to {to}", key, from, to);
        KvCas<T, U> cas = new(key, from, to);
        var response = await _node.RpcAsync(_serviceName, cas);
        switch (response.Body.Type)
        {
            case ErrorBody.ErrorBodyType:
                response.DeserializeAs<ErrorBody>();
                var error = (ErrorBody)response.Body;
                logger.LogDebug("Error setting key {key}: {errorCode} {errorText}", key, error.ErrorCode, error.ErrorText);

                switch (error.ErrorCode)
                {
                    case Models.ErrorCodes.KeyDoesNotExist:
                        throw new KvStoreKeyNotFoundException($"Key {key} does not exist");
                    case Models.ErrorCodes.PreconditionFailed:
                        throw new KvStoreCasPreconditionFailed($"CAS precondition failed for key {key}");
                    default:
                        throw new KvStoreException($"Error setting key {key}: {error.ErrorText}");
                }

            case KvCasOk.CasOkType:
                logger.LogDebug("CAS key {key} from {from} to {to} succeeded", key, from, to);
                break;

            default:
                throw new Exception($"Unexpected return type for CAS operation: {response.Body.Type}");
        }
    }
}

internal class KvStoreException(string message) : Exception(message)
{
}

internal class KvStoreKeyNotFoundException(string message) : KvStoreException(message)
{
}

internal class KvStoreCasPreconditionFailed(string message) : KvStoreException(message)
{
}