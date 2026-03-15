namespace sNet.CScriptPro;

public enum CsrId
{
	Semicolon,
	OpenParen,
	CloseParen,
	OpenBrace,
	CloseBrace,
	OpenSquare,
	CloseSquare,
	Comma,
	
	// Operator
	
	Invoke,
	Minus,
	Complement,
	Not,
	Period,
	
	Mul,
	Div,
	Rem,
	Add,
	Sub,
	
	ShiftLeft,
	ShiftRight,
	
	LessThan,
	GreaterThan,
	LessThanOrEqual,
	GreaterThanOrEqual,
	
	Equals,
	NotEquals,
	
	And,
	Or,
	Xor,
	
	Assign,
	
	// Special
	Literal,
	Identifier,
	ArrayDefinition,
	TableDefinition,
	
	// Keywords
	Let,
	Const,
	Function,
	If,
	Else,
	While,
	Return,
	For,
	Break,
	Continue,
	Import,
	As,
	Include,
	Cast,
	Typeof,
	DynamicCast,
}