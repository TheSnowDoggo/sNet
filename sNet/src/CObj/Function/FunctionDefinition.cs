namespace sNet.CScriptPro;

public sealed class FunctionDefinition : Obj
{
	public override TypeId TypeId => TypeId.Nil;

	public Arg[] Args { get; init; }
	public List<Statement> Statements { get; init;  }
	public string Name { get; set; }
	
	public static FunctionDefinition ParseMain(CsrTokenStream stream)
	{
		var statements = new List<Statement>();

		while (!stream.EndOfStream)
		{
			statements.Add(Statement.Parse(stream));
		}

		return new FunctionDefinition()
		{
			Args = [],
			Statements = statements,
		};
	}

	public static FunctionDefinition ParseMain(string filepath)
	{
		var tokens = CsrTokenizer.TokenizeFile(filepath);
		return ParseMain(tokens);
	}
	
	public static FunctionDefinition ParseLambda(CsrTokenStream stream)
	{
		stream.Consume(CsrId.OpenParen);

		var argNames = new HashSet<string>();
		var args = new List<Arg>();
		
		while (!stream.EndOfStream && stream.Peek().Type != CsrId.CloseParen)
		{
			var name = stream.Consume(CsrId.Identifier).Lexeme;

			if (!argNames.Add(name))
			{
				throw new ParserException(stream.Line, $"Argument list contained duplicate name {name}.");
			}

			Obj defaultValue = Nil.Value;

			if (stream.Peek().Type == CsrId.Assign)
			{
				stream.Read();
				defaultValue = stream.Consume(CsrId.Literal).Value;
			}
			
			args.Add(new Arg(name, defaultValue));

			if (stream.Peek().Type == CsrId.CloseParen)
			{
				break;
			}

			stream.Consume(CsrId.Comma);
		}

		stream.Consume(CsrId.CloseParen);

		var statements = BlockStatement.Parse(stream).Statements;

		return new FunctionDefinition()
		{
			Args = args.ToArray(),
			Statements = statements,
			Name = "__anonymous",
		};
	}

	public static DefineStatement ParseStatement(CsrTokenStream stream)
	{
		int line = stream.Peek().Line;
		
		stream.Consume(CsrId.Function);

		var name = stream.Consume(CsrId.Identifier).Lexeme;
		
		var definition = ParseLambda(stream);
		definition.Name = name;

		return new DefineStatement(line)
		{
			Name = name,
			Attributes = VariableAttribute.Const,
			Expression = [new CsrToken(line, CsrId.Function, "function_def", definition)],
		};
	}

	public UserFunction Create(Context context = null) => new UserFunction()
	{
		Parent = context,
		Args = Args,
		Statements = Statements,
		RuntimeName = Name,
		MinArgs = 0,
		MaxArgs = Args.Length,
	};

	public CsrToken ToToken(int line)
	{
		return new CsrToken(line, CsrId.Function, "function_def", this);
	}
}