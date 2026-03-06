using System.Collections.Frozen;
using System.Text;

namespace sNet.CScriptPro;

public sealed class PartTokenizer : Tokenizer<PartId, PartToken>
{
    private static readonly FrozenDictionary<char, PartId> Single = new Dictionary<char, PartId>()
    {
        { '(', PartId.OpenParen },
        { ')', PartId.CloseParen },
        { '{', PartId.OpenBrace },
        { '}', PartId.CloseBrace },
        { ';', PartId.Semicolon },
        { ':', PartId.Colon },
        { ',', PartId.Comma },
        { '!', PartId.Bang },
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, PartId> Keywords = new Dictionary<string, PartId>()
    {
        { "Vec2", PartId.Vec2 },
    }.ToFrozenDictionary();
    
    public PartTokenizer(TextReader reader)
        : base(reader) { }
    
    public PartTokenizer(Stream stream)
        : base(new StreamReader(stream, Encoding.UTF8)) { }

    public static List<PartToken> TokenizeFile(string filepath)
    {
        using var fs = File.OpenRead(filepath);
        return new PartTokenizer(fs).Tokenize();
    }
    
    protected override IReadOnlyDictionary<char, PartId> SingleMap => Single;
    protected override IReadOnlyDictionary<string, PartId> KeywordsMap => Keywords;

    protected override PartId IdentifierId => PartId.Identifier;
    protected override PartId LiteralId => PartId.Literal;

    protected override PartToken Create(int line, PartId id, string lexeme, CObj value = null)
    {
        return new PartToken(line, id, lexeme, value ?? Nil.Value);
    }
}