namespace sNet.CScriptPro;

public abstract class Statement
{
    public Statement(int line)
    {
        Line = line;
    }
    
    public int Line { get; }

    public static Statement Parse(CsrTokenStream stream)
    {
        var head = stream.Peek();

        return head.Type switch
        {
            CsrId.Let or CsrId.Const
                => DefineStatement.Parse(stream),
            CsrId.Identifier
                => ExpressionStatement.Parse(stream),
            CsrId.OpenBrace
                => BlockStatement.Parse(stream),
            CsrId.If
                => IfStatement.Parse(stream),
            CsrId.While
                => WhileStatement.Parse(stream),
            CsrId.Return
                => ReturnStatement.Parse(stream),
            CsrId.Function
                => FunctionDefinition.ParseStatement(stream),
            CsrId.For
                => ForStatement.Parse(stream),
            CsrId.Foreach
                => ForeachStatement.Parse(stream),
            CsrId.Break
                => ValueStatement.Parse(stream, ReturnType.Break),
            CsrId.Continue
                => ValueStatement.Parse(stream, ReturnType.Continue),
            CsrId.Import
                => ImportStatement.Parse(stream),
            CsrId.Include
                => IncludeStatement.Parse(stream),
            _ => throw new ParserException(head.Line, $"Unrecognised start of statement: {head.Type}."),
        };
    }

    public virtual ReturnValue Run(Context context)
    {
        context.Line = Line;
        return ReturnValue.None;
    }
}