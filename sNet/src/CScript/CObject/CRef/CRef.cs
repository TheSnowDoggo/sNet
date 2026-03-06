namespace sNet.CScriptPro;

public abstract class CRef : CObj
{
    protected CRef() : base(TypeId.Nil) { }
    
    public abstract CObj Value { get; set; }
}