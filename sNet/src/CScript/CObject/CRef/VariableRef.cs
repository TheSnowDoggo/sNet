namespace sNet.CScriptPro;

public sealed class VariableRef : CRef
{
    private readonly Context _context;
    
    public VariableRef(string name, Context context)
    {
        Name = name;
        _context = context;
    }
    
    public string Name { get; }

    public override CObj Value
    {
        get => _context[Name];
        set => _context[Name] = value;
    }
}