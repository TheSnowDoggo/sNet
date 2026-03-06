namespace sNet.CScriptPro;

public sealed class AddNetPack
{
    private Queue<Part> _queue = [];

    public Queue<Part> Queue => _queue;

    public static Queue<Part> Deserialize(Stream stream)
    {
        int count = stream.ReadNetInt32();

        if (count < 0)
        {
            throw new InvalidDataException($"Queue count (\'{count}\') was negative.");
        }

        var queue = new Queue<Part>();

        for (int i = 0; i < count; i++)
        {
            queue.Enqueue(stream.ReadPart());
        }

        return queue;
    }

    public void Serialize(NetSerializer serial)
    {
        var queue = Interlocked.Exchange(ref _queue, []);
        
        serial.WriteInt32(queue.Count);

        while (queue.TryDequeue(out var part))
        {
            serial.WritePart(part);
        }
    }
}