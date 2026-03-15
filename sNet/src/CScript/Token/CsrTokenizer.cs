using System.Collections.Frozen;
using System.Text;

namespace sNet.CScriptPro;

public sealed class CsrTokenizer : Tokenizer<CsrId, CsrToken>
{
	public CsrTokenizer(TextReader reader)
		: base(reader) { }

	public CsrTokenizer(Stream stream)
		: base(new StreamReader(stream, Encoding.UTF8)) { }

	protected override IReadOnlyDictionary<char, CsrId> SingleOperators => CsrConfig.SingleOperators;
	protected override IReadOnlyDictionary<string, CsrId> DoubleOperators => CsrConfig.DoubleOperators;

	protected override IReadOnlySet<CsrId> CompoundSet => CsrConfig.Compound;

	protected override IReadOnlyDictionary<string, Obj> LiteralMap => CsrConfig.Literals;
	protected override IReadOnlyDictionary<string, CsrId> KeywordsMap => CsrConfig.Keywords;

	protected override CsrId LiteralId => CsrId.Literal;
	protected override CsrId IdentifierId => CsrId.Identifier;

	protected override CsrToken Create(int line, CsrId id, string lexeme, Obj value = null)
	{
		return new CsrToken(line, id, lexeme, value ?? Nil.Value);
	}

	public static List<CsrToken> TokenizeFile(string filename)
	{
		using var stream = File.OpenRead(filename);
		return new CsrTokenizer(stream).Tokenize();
	}
}