namespace sNet.Service.Part;

public sealed class ServerPartService : ServerService
{
    public const int MaxAddSize    = 4096;
    public const int MaxUpdateSize = 4096;
    public const int MaxRemoveSize = 1024;
    
    public override ServiceId ServiceId => ServiceId.Part;

    public void FireBroadcast(PartSid sid, INetSerializable data, int maxSize)
    {
        Task.Run(() => BroadcastAsync(sid, data, maxSize));
    }
    
    public async Task<bool> BroadcastAsync(PartSid sid, INetSerializable data, int maxSize)
    {
        try
        {
            using var buffer = Format(sid, data, maxSize);
            return await Server.BroadcastAsync(buffer);
        }
        catch (Exception ex)
        {
            Logger.Error(ex.Message);
            return false;
        }
    }
    
    private RentBuffer Format(PartSid sid, INetSerializable data, int maxSize)
    {
        var buffer = RentBuffer.Share(maxSize);

        try
        {
            using var serial = buffer.OpenSerial();
            
            serial.Begin();
            
            serial.WriteByte((byte)ServiceId);
            serial.WriteByte((byte)sid);
            data.Serialize(serial);
            
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