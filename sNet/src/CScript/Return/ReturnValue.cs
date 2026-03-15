namespace sNet.CScriptPro;

public sealed class ReturnValue
{
    public ReturnValue(ReturnType type, Obj value = null)
    {
        Type = type;
        Value = value;
    }
    
    public static ReturnValue None { get; } = new ReturnValue(ReturnType.None);
    public static ReturnValue Break { get; } = new ReturnValue(ReturnType.Break);
    public static ReturnValue Continue { get; } = new ReturnValue(ReturnType.Continue);
    
    public ReturnType Type { get; }
    public Obj Value { get; }

    public static bool TryExit(ref ReturnValue returnValue)
    {
        switch (returnValue.Type)
        {
        case ReturnType.Return:
            return true;
        case ReturnType.Break:
            returnValue = None;
            return true;
        default:
            return false;
        }
    }
    
    public static bool TryExit(ref ReturnValue returnValue, Context context)
    {
        if (!TryExit(ref returnValue))
        {
            return false;
        }
        
        context.CloseScope();
        return true;
    }
}