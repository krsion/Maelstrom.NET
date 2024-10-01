using MaelstromNode.Interfaces;
using MaelstromNode.Models;
using MaelstromNode.Models.MessageBodies;

namespace MaelstromNode.Workloads;

class KafkaLog(ILogger<KafkaLog> logger, IReceiver receiver, ISender sender) : MaelstromNode(logger, receiver, sender)
{
    private const int _maxReturnedMessages = 10;
    protected new ILogger<KafkaLog> logger = logger;
    private readonly Dictionary<string, List<int>> _log = [];
    private readonly SemaphoreSlim _logLock = new(1);
    private readonly Dictionary<string, int> _committedOffsets = [];
    private readonly SemaphoreSlim _committedOffsetsLock = new(1);

    [MaelstromHandler(Send.SendType)]
    public async Task HandleSend(Message message)
    {
        message.DeserializeAs<Send>();
        var send = (Send)message.Body;
        logger.LogInformation("Received send request: {Key} {Message}", send.Key, send.Message);
        await _logLock.WaitAsync();
        try
        {
            if (!_log.TryGetValue(send.Key, out List<int>? value))
            {
                value = [];
                _log[send.Key] = value;
            }

            value.Add(send.Message);
        }
        finally
        {
            _logLock.Release();
        }
        await ReplyAsync(message, new SendOk(GetLatestOffset(send.Key)));
    }

    [MaelstromHandler(Poll.PollType)]
    public async Task HandlePoll(Message message)
    {
        message.DeserializeAs<Poll>();
        var poll = (Poll)message.Body;
        logger.LogInformation("Received poll request: {Offsets}", poll.Offsets);
        Dictionary<string, List<List<int>>> messages;
        await _logLock.WaitAsync();
        try
        {
            messages = _log
                .Where(kv => poll.Offsets.ContainsKey(kv.Key))
                .Select(kv => (kv.Key, GetMessagesFromOffset(kv.Key, poll.Offsets[kv.Key])))
                .ToDictionary();
        }
        finally
        {
            _logLock.Release();
        }

        await ReplyAsync(message, new PollOk(messages));
    }

    [MaelstromHandler(CommitOffsets.CommitOffsetsType)]
    public async Task HandleCommitOffsets(Message message)
    {
        message.DeserializeAs<CommitOffsets>();
        var commitOffsets = (CommitOffsets)message.Body;
        logger.LogInformation("Received commit offsets request: {Offsets}", commitOffsets.Offsets);
        await _committedOffsetsLock.WaitAsync();
        try
        {
            foreach (var kv in commitOffsets.Offsets)
            {
                _committedOffsets[kv.Key] = kv.Value;
            }
        }
        finally
        {
            _committedOffsetsLock.Release();
        }

        await ReplyAsync(message, new CommitOffsetsOk());
    }

    [MaelstromHandler(ListCommittedOffsets.ListCommittedOffsetsType)]
    public async Task HandleListCommittedOffsets(Message message)
    {
        message.DeserializeAs<ListCommittedOffsets>();
        var listCommittedOffsets = (ListCommittedOffsets)message.Body;
        logger.LogInformation("Received list committed offsets request: {Keys}", listCommittedOffsets.Keys);
        Dictionary<string, int> committedOffsets;
        await _committedOffsetsLock.WaitAsync();
        try
        {
            committedOffsets = listCommittedOffsets.Keys
                .Where(_committedOffsets.ContainsKey)
                .Select(k => new KeyValuePair<string, int>(k, _committedOffsets[k]))
                .ToDictionary();
        }
        finally
        {
            _committedOffsetsLock.Release();
        }

        await ReplyAsync(message, new ListCommittedOffsetsOk(committedOffsets));
    }

    private List<List<int>> GetMessagesFromOffset(string key, int offset) => _log[key]
        .Select((v, ix) => new List<int> { ix, v })
        .Skip(offset)
        .Take(_maxReturnedMessages)
        .ToList();

    private int GetLatestOffset(string key) => _log[key].Count - 1;
}
