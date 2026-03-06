namespace sNet.CScriptPro;

public sealed class Variable
{
    public Variable(Obj value, VariableAttribute attributes = VariableAttribute.None)
    {
        Value = value;
        Attributes = attributes;
    }
    
    public Obj Value { get; set; }
    public VariableAttribute Attributes { get; }
}