namespace sNet.CScriptPro;

public sealed class ReturnValue
{
    public ReturnValue(ReturnType type, CObj value = null)
    {
        Type = type;
        Value = value;
    }
    
    public static ReturnValue None { get; } = new ReturnValue(ReturnType.None);
    public static ReturnValue Break { get; } = new ReturnValue(ReturnType.Break);
    public static ReturnValue Continue { get; } = new ReturnValue(ReturnType.Continue);
    
    public ReturnType Type { get; }
    public CObj Value { get; }
}