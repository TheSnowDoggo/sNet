namespace sNet.CScriptPro;

public sealed class PartStream : TokenStream<PartId, PartToken>
{
    public PartStream(List<PartToken> tokens, int start = 0)
        : base(tokens, start) { }

    public static implicit operator PartStream(List<PartToken> tokens) => new PartStream(tokens);
}