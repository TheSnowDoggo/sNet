namespace sNet.CScriptPro;

public sealed class Variable
{
    public Variable(CObj value, VariableAttribute attributes = VariableAttribute.None)
    {
        Value = value;
        Attributes = attributes;
    }
    
    public CObj Value { get; set; }
    public VariableAttribute Attributes { get; }
}