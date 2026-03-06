namespace sNet.CScriptPro;

public sealed class AddNetPack : INetSerializable
{
    private Queue<Part> _queue = [];

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

        var queue = new Queue<AddEvent>();

        for (int i = 0; i < count; i++)
        {
            var parent = (Uid)stream.ReadUid();
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
            serial.WriteUid(part.Parent?.Uid ?? Uid.Null);
            serial.WritePart(part);
        }
    }
}