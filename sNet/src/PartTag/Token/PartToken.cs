namespace sNet.CScriptPro;

public sealed class PartToken : Token<PartId>
{
    public PartToken(int line, PartId id, string lexeme, Obj value)
        : base(line, id, lexeme, value) { }
}