namespace sNet.CScriptPro;

public abstract class RefObj : Obj
{
    protected RefObj() : base(TypeId.Nil) { }
    
    public abstract Obj Value { get; set; }
}