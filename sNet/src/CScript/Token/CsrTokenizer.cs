using System.Collections.Frozen;
using System.Text;

namespace sNet.CScriptPro;

public sealed class CsrTokenizer : Tokenizer<CsrId, CsrToken>
{
	private static readonly FrozenDictionary<string, CsrId> Double = new Dictionary<string, CsrId>()
	{
		{ "<=", CsrId.LessThanOrEqual },
		{ ">=", CsrId.GreaterThanOrEqual },
		{ "==", CsrId.Equals },
		{ "!=", CsrId.NotEquals },
		{ "<<", CsrId.ShiftLeft },
		{ ">>", CsrId.ShiftRight },
	}.ToFrozenDictionary();
	
	private static readonly FrozenDictionary<char, CsrId> Single = new Dictionary<char, CsrId>()
	{
		{ ';', CsrId.Semicolon },
		{ '(', CsrId.OpenParen },
		{ ')', CsrId.CloseParen },
		{ '{', CsrId.OpenBrace },
		{ '}', CsrId.CloseBrace },
		{ '[', CsrId.OpenSquare },
		{ ']', CsrId.CloseSquare },
		{ '.', CsrId.Period },
		{ ',', CsrId.Comma },
		{ '~', CsrId.Complement },
		{ '!', CsrId.Not },
		{ '*', CsrId.Mul },
		{ '/', CsrId.Div },
		{ '%', CsrId.Rem },
		{ '+', CsrId.Add },
		{ '-', CsrId.Sub },
		{ '<', CsrId.LessThan },
		{ '>', CsrId.GreaterThan },
		{ '&', CsrId.And },
		{ '^', CsrId.Xor },
		{ '|', CsrId.Or },
		{ '=', CsrId.Assign },
	}.ToFrozenDictionary();

	public static readonly FrozenSet<CsrId> Compound = new HashSet<CsrId>()
	{
		CsrId.Mul,
		CsrId.Div,
		CsrId.Rem,
		CsrId.Add,
		CsrId.Sub,
		CsrId.ShiftLeft,
		CsrId.ShiftRight,
		CsrId.And,
		CsrId.Xor,
		CsrId.Or,
	}.ToFrozenSet();
	
	private static readonly FrozenDictionary<string, Obj> Literals = new Dictionary<string, Obj>()
	{
		{ "nil", Nil.Value },
		{ "true", Bool.True },
		{ "false", Bool.False },
	}.ToFrozenDictionary();
	
	private static readonly FrozenDictionary<string, CsrId> Keywordses = new Dictionary<string, CsrId>()
	{
		{ "let", CsrId.Let },
		{ "const", CsrId.Const },
		{ "function", CsrId.Function },
		{ "if", CsrId.If },
		{ "else", CsrId.Else },
		{ "while", CsrId.While },
		{ "return", CsrId.Return },
		{ "for", CsrId.For },
		{ "break", CsrId.Break },
		{ "continue", CsrId.Continue },
		{ "import", CsrId.Import },
		{ "cast", CsrId.Cast },
		{ "typeof", CsrId.Typeof },
		{ "dynamic_cast", CsrId.DynamicCast },
		{ "as", CsrId.As },
		{ "and", CsrId.And },
		{ "or", CsrId.Or },
		{ "xor", CsrId.Xor },
		{ "not", CsrId.Not },
	}.ToFrozenDictionary();
	
	public CsrTokenizer(TextReader reader)
		: base(reader) { }

	public CsrTokenizer(Stream stream)
		: base(new StreamReader(stream, Encoding.UTF8)) { }

	protected override IReadOnlyDictionary<char, CsrId> SingleMap => Single;
	protected override IReadOnlyDictionary<string, CsrId> DoubleMap => Double;

	protected override IReadOnlySet<CsrId> CompoundSet => Compound;

	protected override IReadOnlyDictionary<string, Obj> LiteralMap => Literals;
	protected override IReadOnlyDictionary<string, CsrId> KeywordsMap => Keywordses;

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