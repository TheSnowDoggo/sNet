using System.Collections.Frozen;
using System.Text;

namespace sNet.CScriptPro;

public sealed class PartTokenizer : Tokenizer<PartId, PartToken>
{
    public PartTokenizer(TextReader reader)
        : base(reader) { }
    
    public PartTokenizer(Stream stream)
        : base(new StreamReader(stream, Encoding.UTF8)) { }

    public static List<PartToken> TokenizeFile(string filepath)
    {
        using var fs = File.OpenRead(filepath);
        return new PartTokenizer(fs).Tokenize();
    }
    
    protected override IReadOnlyDictionary<char, PartId> SingleOperators => TagConfig.SingleOperators;
    protected override IReadOnlyDictionary<string, PartId> KeywordsMap => TagConfig.Keywords;

    protected override PartId IdentifierId => PartId.Identifier;
    protected override PartId LiteralId => PartId.Literal;

    protected override PartToken Create(int line, PartId id, string lexeme, Obj value = null)
    {
        return new PartToken(line, id, lexeme, value ?? Nil.Value);
    }
}