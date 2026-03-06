namespace sNet.CScriptPro;

public sealed class VariableRefObj : RefObj
{
    private readonly Context _context;
    
    public VariableRefObj(string name, Context context)
    {
        Name = name;
        _context = context;
    }
    
    public string Name { get; }

    public override Obj Value
    {
        get => _context[Name];
        set => _context[Name] = value;
    }
}