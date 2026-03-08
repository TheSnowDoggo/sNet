using sNet.CScriptPro;
using sNet.Server;

namespace sNet.Service.Part;

public sealed class ServerPartService : ServerService
{
    public ServerPartRoot Root { get; set; }
    
    public override ServiceId ServiceId => ServiceId.Part;

    public override void Receive(ServerNetCall call)
    {
        var sid = (PartSid)call.Stream.ReadExactByte();

        switch (sid)
        {
        case PartSid.FireServer:
            HandleFireServer(call);
            break;
        default:
            Logger.Error($"Unrecognised part sid: {sid}.");
            break;
        }
    }

    public override void ClientJoined(RemoteClient client)
    {
        var add = new AddPack();

        foreach (var child in Root.Root.Children)
        {
            add.Enqueue(child);
        }
        
        Task.Run(() => SendPackAsync(client.Idx, (byte)PartSid.Add, add));
        
        Logger.Info($"Sending add package to {client}.");
    }

    public async Task<bool> BroadcastAdd(AddPack add)
    {
        return await BroadcastPackAsync((byte)PartSid.Add, add);
    }
    
    public async Task<bool> BroadcastRemove(RemovePack remove)
    {
        return await BroadcastPackAsync((byte)PartSid.Remove, remove);
    }
    
    public async Task<bool> BroadcastUpdate(UpdatePack update)
    {
        return await BroadcastPackAsync((byte)PartSid.Update, update);
    }

    public async Task<bool> BroadcastEvent(EventPack events)
    {
        return await BroadcastPackAsync((byte)PartSid.FireClient, events);
    }

    public async Task<bool> SendEvent(int idx, EventPack events)
    {
        return await SendPackAsync(idx, (byte)PartSid.FireClient, events);
    }

    private void HandleFireServer(ServerNetCall call)
    {
        if (Root == null)
        {
            Logger.Error("Failed to handle event: No root assigned.");
            return;
        }
        
        Root.QueueEvents(call.Stream, call.Client.Idx);
    }
}