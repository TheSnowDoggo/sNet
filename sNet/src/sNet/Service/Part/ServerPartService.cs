using sNet.CScriptPro;
using sNet.Server;

namespace sNet.Service.Part;

public sealed class ServerPartService : ServerService
{
    public ServerPartRoot Root { get; set; }
    
    public override ServiceId ServiceId => ServiceId.Part;

    public override void ClientJoined(RemoteClient client)
    {
        var add = new AddNetPack();

        foreach (var child in Root.Root.Children)
        {
            add.Enqueue(child);
        }
        
        Task.Run(() => SendPackAsync(client.Idx, (byte)PartSid.Add, add));
        
        Logger.Info($"Sending add package to {client}.");
    }

    public async Task<bool> BroadcastAdd(AddNetPack add)
    {
        return await BroadcastPackAsync((byte)PartSid.Add, add);
    }
    
    public async Task<bool> BroadcastRemove(RemoveNetPack remove)
    {
        return await BroadcastPackAsync((byte)PartSid.Remove, remove);
    }
    
    public async Task<bool> BroadcastUpdate(UpdateNetPack update)
    {
        return await BroadcastPackAsync((byte)PartSid.Update, update);
    }
}