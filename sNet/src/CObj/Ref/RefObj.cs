namespace sNet.CScriptPro;

public abstract class RefObj : Obj
{
    public override TypeId TypeId => TypeId.Nil;

    public abstract Obj Value { get; set; }
}