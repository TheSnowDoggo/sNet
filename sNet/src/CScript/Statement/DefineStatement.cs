namespace sNet.CScriptPro;

public sealed class DefineStatement : Statement
{
    public DefineStatement(int line) : base(line) { }
    
    public string Name { get; init; }
    public VariableAttribute Attributes { get; init; }
    public List<CsrToken> Expression { get; init; }

    public new static DefineStatement Parse(CsrTokenStream stream)
    {
        var head = stream.Read();

        var attributes = head.Type switch
        {
            CsrId.Let => VariableAttribute.None,
            CsrId.Const => VariableAttribute.Const,
            _ => throw new ParserException(head.Line, $"Unexpected token {head.Type}, expected either let or const.")
        };

        var name = stream.Consume(CsrId.Identifier).Lexeme;

        stream.Consume(CsrId.Assign);

        var expr = new RpnParser(stream).Parse([CsrId.Semicolon]);
        
        stream.Consume(CsrId.Semicolon);

        return new DefineStatement(head.Line)
        {
            Attributes = attributes,
            Name = name,
            Expression = expr,
        };
    }

    public override ReturnValue Run(Context context)
    {
        base.Run(context);
        
        var value = new Evaluator(context, Expression).Evaluate();
        context.Define(Name, value, Attributes);
        
        return ReturnValue.None;
    }
}