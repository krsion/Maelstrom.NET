namespace Maelstrom.Interfaces;

public interface IKvStoreClient
{
    Task CasAsync<T, U>(T key, U from, U to, bool createIfNotExists = false);
    Task<U> ReadAsync<T, U>(T key);
    Task WriteAsync<T, U>(T key, U value);
}