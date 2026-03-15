using System.Collections.Frozen;

namespace sNet.CScriptPro;

public static class CsrConfig
{
	public const int AssignPrecedence = 0;
	
	public static readonly FrozenDictionary<string, CsrId> DoubleOperators = new Dictionary<string, CsrId>()
	{
		{ "<=", CsrId.LessThanOrEqual },
		{ ">=", CsrId.GreaterThanOrEqual },
		{ "==", CsrId.Equals },
		{ "!=", CsrId.NotEquals },
		{ "<<", CsrId.ShiftLeft },
		{ ">>", CsrId.ShiftRight },
	}.ToFrozenDictionary();
	
	public static readonly FrozenDictionary<char, CsrId> SingleOperators = new Dictionary<char, CsrId>()
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
	
	public static readonly FrozenDictionary<string, Obj> Literals = new Dictionary<string, Obj>()
	{
		{ "nil", Nil.Value },
		{ "true", Bool.True },
		{ "false", Bool.False },
	}.ToFrozenDictionary();
	
	public static readonly FrozenDictionary<string, CsrId> Keywords = new Dictionary<string, CsrId>()
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
		{ "as", CsrId.As },
		{ "include", CsrId.Include },
		{ "cast", CsrId.Cast },
		{ "typeof", CsrId.Typeof },
		{ "dynamic_cast", CsrId.DynamicCast },
		{ "and", CsrId.And },
		{ "or", CsrId.Or },
		{ "xor", CsrId.Xor },
		{ "not", CsrId.Not },
	}.ToFrozenDictionary();
	
	public static readonly FrozenDictionary<CsrId, int> Precedence = new Dictionary<CsrId, int>()
	{
		{ CsrId.Invoke, 12 },
		{ CsrId.Period, 12 },
		{ CsrId.DynamicCast, 12 },
		{ CsrId.Complement, 11 },
		{ CsrId.Not, 11 },
		{ CsrId.Minus, 11 },
		{ CsrId.Cast, 11 },
		{ CsrId.Typeof, 11 },
		{ CsrId.Mul, 10 },
		{ CsrId.Div, 10 },
		{ CsrId.Rem, 10 },
		{ CsrId.Add, 9 },
		{ CsrId.Sub, 9 },
		{ CsrId.ShiftLeft, 8 },
		{ CsrId.ShiftRight, 8 },
		{ CsrId.LessThan, 7 },
		{ CsrId.GreaterThan, 7 },
		{ CsrId.LessThanOrEqual, 7 },
		{ CsrId.GreaterThanOrEqual, 7 },
		{ CsrId.Equals, 6 },
		{ CsrId.NotEquals, 6 },
		{ CsrId.And, 5 },
		{ CsrId.Xor, 2 },
		{ CsrId.Or, 3 },
		{ CsrId.Assign, AssignPrecedence },
		{ CsrId.OpenParen, int.MinValue },
		{ CsrId.Comma, int.MinValue },
	}.ToFrozenDictionary();

	public static readonly FrozenDictionary<CsrId, CsrId> UnaryMap = new Dictionary<CsrId, CsrId>()
	{
		{ CsrId.Sub, CsrId.Minus },
	}.ToFrozenDictionary();

	public static readonly FrozenSet<CsrId> RightAssociative = new HashSet<CsrId>()
	{
		CsrId.Complement,
		CsrId.Not,
		CsrId.Minus,
		CsrId.Cast,
		CsrId.Typeof,
	}.ToFrozenSet();
}