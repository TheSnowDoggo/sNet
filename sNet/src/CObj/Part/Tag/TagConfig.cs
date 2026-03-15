using System.Collections.Frozen;

namespace sNet.CScriptPro;

public static class TagConfig
{
	public static readonly FrozenDictionary<char, PartId> SingleOperators = new Dictionary<char, PartId>()
	{
		{ '(', PartId.OpenParen },
		{ ')', PartId.CloseParen },
		{ '{', PartId.OpenBrace },
		{ '}', PartId.CloseBrace },
		{ '$', PartId.Dollar },
		{ '%', PartId.Percentage },
		{ ';', PartId.Semicolon },
		{ ':', PartId.Colon },
		{ ',', PartId.Comma },
		{ '!', PartId.Bang },
		{ '.', PartId.Period },
	}.ToFrozenDictionary();

	public static readonly FrozenDictionary<string, PartId> Keywords = new Dictionary<string, PartId>()
	{
		{ "Vec2", PartId.Vec2 },
		{ "Key", PartId.Key },
		{ "Color", PartId.Color },
	}.ToFrozenDictionary();
}