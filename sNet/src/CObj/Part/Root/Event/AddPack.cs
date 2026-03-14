namespace sNet.CScriptPro;

public sealed class AddPack : INetPackage
{
    public const int MaxAddSize = 4096;
    
    private Queue<Part> _queue = [];

    public bool IsEmpty => _queue.Count == 0;
    
    public int MaxSize => MaxAddSize * _queue.Count;

    public void Enqueue(Part part)
    {
        _queue.Enqueue(part);
    }

    public static Queue<AddEvent> Deserialize(Stream stream)
    {
        int count = stream.ReadNetInt32();

        if (count < 0)
        {
            throw new InvalidDataException($"Queue count (\'{count}\') was negative.");
        }

        var queue = new Queue<AddEvent>(count);

        for (int i = 0; i < count; i++)
        {
            var parent = (Uid)stream.ReadNetInt64();
            var part = stream.ReadPart();
            
            queue.Enqueue(new AddEvent(parent, part));
        }

        return queue;
    }

    public void Serialize(NetSerializer serial)
    {
        var queue = Interlocked.Exchange(ref _queue, []);
        
        serial.WriteInt32(queue.Count);
    
        while (queue.TryDequeue(out var part))
        {
            serial.WriteInt64(part.Parent?.Uid ?? Uid.Null);
            serial.WritePart(part);
        }
    }
}