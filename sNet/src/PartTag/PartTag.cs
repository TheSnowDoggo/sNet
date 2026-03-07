using System.Text;

namespace sNet.CScriptPro;

public sealed class PartTag
{
    public PartTag(PartType partType)
    {
        PartType = partType;
    }
    
    public PartType PartType { get; }
    public Dictionary<string, Obj> Properties { get; } = [];
    public List<PartTag> Children { get; } = [];
    
    public static PartTag Parse(PartStream stream)
    {
        stream.Consume(PartId.Bang);
        
        var partStr = stream.Consume(PartId.Identifier).Lexeme;
        
        if (!Enum.TryParse<PartType>(partStr, true, out var partId))
        {
            throw new ParserException(stream.Line, $"Unrecognised part id {partStr}.");
        }
        
        var tag = new PartTag(partId);
        
        stream.Consume(PartId.OpenBrace);

        while (!stream.EndOfStream() && stream.Peek().Type != PartId.CloseBrace)
        {
            var head = stream.Peek();
            
            switch (head.Type)
            {
            case PartId.Identifier:
                stream.Read();
                
                stream.Consume(PartId.Colon);

                var value = ParseValue(stream);

                if (!tag.Properties.TryAdd(head.Lexeme, value))
                {
                    throw new ParserException(stream.Line, $"Tag contains duplicate property name (\'{head.Lexeme}\').");
                }

                stream.Consume(PartId.Semicolon);
                break;
            case PartId.Bang:
                tag.Children.Add(Parse(stream));
                break;
            default:
                throw new ParserException(stream.Line, $"Expected start of child tag or property value, got {head.Type}.");
            }
        }

        stream.Consume(PartId.CloseBrace);

        return tag;
    }
    
    public static PartTag Parse(string filepath)
    {
        return Parse(PartTokenizer.TokenizeFile(filepath));
    }

    public static List<PartTag> ParseAll(PartStream stream)
    {
        var tags = new List<PartTag>();

        while (!stream.EndOfStream())
        {
            tags.Add(Parse(stream));
        }
        
        return tags;
    }

    public static List<PartTag> ParseAll(string filepath)
    {
        return ParseAll(PartTokenizer.TokenizeFile(filepath));
    }

    public Part Create()
    {
        var part = Part.Create(PartType);

        foreach (var property in Properties)
        {
            part[property.Key] = property.Value;
        }

        foreach (var child in Children)
        {
            part.AddChild(child.Create());
        }
        
        return part;
    }

    private static Obj ParseValue(PartStream stream) => stream.Peek().Type switch
    {
        PartId.Literal => stream.Read().Value,
        PartId.Vec2 => ParseVec2(stream),
        _ => throw new ParserException(stream.Line, $"Unrecognised value type {stream.Peek().Type}."),
    };

    private static Vec2Obj ParseVec2(PartStream stream)
    {
        stream.Consume(PartId.Vec2);
        stream.Consume(PartId.OpenParen);

        var x = stream.Consume(PartId.Literal).Value;
        x.Expect(TypeId.Number);

        stream.Consume(PartId.Comma);

        var y = stream.Consume(PartId.Literal).Value;
        y.Expect(TypeId.Number);
        
        stream.Consume(PartId.CloseParen);

        return new Vec2Obj((Number)x, (Number)y);
    }
    
    private void Format(StringBuilder sb, int level)
    {
        sb.Append(' ', level * 2);
        sb.AppendLine($"!{PartType}");
        
        sb.Append(' ', level * 2);
        sb.AppendLine("{");

        foreach (var kvp in Properties)
        {
            sb.Append(' ', (level + 1) * 2);
            sb.AppendLine($"{kvp.Key}: {kvp.Value};");
        }

        foreach (var child in Children)
        {
            child.Format(sb, level + 1);
            sb.AppendLine();
        }

        sb.Append(' ', level * 2);
        sb.Append('}');
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        Format(sb, 0);
        return sb.ToString();
    }
}