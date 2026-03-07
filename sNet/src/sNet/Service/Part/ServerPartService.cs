using sNet.CScriptPro;
using sNet.Server;

namespace sNet.Service.Part;

public sealed class ServerPartService : ServerService
{
    public const int MaxAddSize    = 4096;
    public const int MaxUpdateSize = 4096;
    public const int MaxRemoveSize = 1024;

    public ServerPartRoot Root { get; set; }
    
    public override ServiceId ServiceId => ServiceId.Part;

    public override void ClientJoined(RemoteClient client)
    {
        var add = new AddNetPack();

        foreach (var child in Root.Root.Children)
        {
            add.Enqueue(child);
        }
        
        Task.Run(() => SendAsync(client.Idx, PartSid.Add, add, MaxAddSize));
        
        Logger.Info($"Sending add package to {client}.");
    }

    public async Task<bool> BroadcastAdd(AddNetPack add)
    {
        return await BroadcastAsync(PartSid.Add, add, MaxAddSize);
    }
    
    public async Task<bool> BroadcastRemove(RemoveNetPack remove)
    {
        return await BroadcastAsync(PartSid.Remove, remove, MaxRemoveSize);
    }
    
    public async Task<bool> BroadcastUpdate(UpdateNetPack update)
    {
        return await BroadcastAsync(PartSid.Update, update, MaxUpdateSize);
    }

    private async Task<bool> BroadcastAsync(PartSid sid, INetSerializable data, int maxSize)
    {
        if (data.IsEmpty) return false;
        return await Server.BroadcastAsync(Format, (sid, data, maxSize));
    }

    private async Task<bool> SendAsync(int idx, PartSid sid, INetSerializable data, int maxSize)
    {
        if (data.IsEmpty) return false;
        return await Server.SendAsync(idx, Format, (sid, data, maxSize));
    }
    
    private RentBuffer Format((PartSid Sid, INetSerializable Data, int MaxSize) state)
    {
        var buffer = RentBuffer.Share(state.MaxSize);

        try
        {
            using var serial = buffer.OpenSerial();
            
            serial.Begin();
            
            serial.WriteByte((byte)ServiceId);
            serial.WriteByte((byte)state.Sid);
            state.Data.Serialize(serial);
            
            serial.End();
            buffer.Trim(serial.WrittenBytes);
            
            return buffer;
        }
        catch
        {
            buffer.Dispose();
            throw;
        }
    }
}