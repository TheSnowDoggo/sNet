using System.Collections.Frozen;
using System.Text;
using SCENeo;
using SCENeo.Input;

namespace sNet.CScriptPro;

public sealed class PartTag
{
    private static readonly FrozenDictionary<string, PartFlag> FlagNames = new Dictionary<string, PartFlag>()
    {
        { "NO_AUTO", PartFlag.NoAuto },
        { "ROOT", PartFlag.Root },
    }.ToFrozenDictionary();
    
    public PartTag(PartType partType)
    {
        PartType = partType;
    }
    
    public PartType PartType { get; }
    public Dictionary<string, Obj> Properties { get; } = [];
    public List<PartTag> Children { get; } = [];
    public PartFlag Flags { get; init; }
    
    public static PartTag Parse(PartStream stream)
    {
        var flags = ParseFlags(stream);
        
        stream.Consume(PartId.Bang);
        
        var partStr = stream.Consume(PartId.Identifier).Lexeme;
        
        if (!Enum.TryParse<PartType>(partStr, true, out var partId))
        {
            throw new ParserException(stream.Line, $"Unrecognised part id {partStr}.");
        }

        var tag = new PartTag(partId)
        {
            Flags = flags,
        };

        tag.ReadSubProperties(stream);
        
        return tag;
    }
    
    public static PartTag Parse(string filepath)
    {
        return Parse(PartTokenizer.TokenizeFile(filepath));
    }

    public static List<PartTag> ParseAll(PartStream stream)
    {
        var tags = new List<PartTag>();

        while (!stream.EndOfStream)
        {
            tags.Add(Parse(stream));
        }
        
        return tags;
    }

    public static List<PartTag> ParseAll(string filepath)
    {
        return ParseAll(PartTokenizer.TokenizeFile(filepath));
    }

    private static PartFlag ParseFlags(PartStream stream)
    {
        var flags = PartFlag.None;
        
        while (!stream.EndOfStream && stream.Peek().Type == PartId.Dollar)
        {
            stream.Read();
            
            var name = stream.Consume(PartId.Identifier).Lexeme;

            if (!FlagNames.TryGetValue(name, out var flag))
            {
                throw new ParserException(stream.Line, $"Unrecognised flag (\'{name}\').");
            }
            
            flags |= flag;
        }

        return flags;
    }

    private void ReadSubProperties(PartStream stream, bool redefine = false)
    {
        if (stream.Peek().Type == PartId.Semicolon)
        {
            stream.Read();
            return;
        }
        
        stream.Consume(PartId.OpenBrace);
        
        while (!stream.EndOfStream && stream.Peek().Type != PartId.CloseBrace)
        {
            var head = stream.Peek();
            
            switch (head.Type)
            {
            case PartId.Identifier:
                stream.Read();
                
                stream.Consume(PartId.Colon);

                var value = ParseValue(stream);

                if (redefine)
                {
                    Properties[head.Lexeme] = value;
                    stream.Consume(PartId.Semicolon);
                    break;
                }
                
                if (!Properties.TryAdd(head.Lexeme, value))
                {
                    throw new ParserException(stream.Line, $"Tag contains duplicate property name (\'{head.Lexeme}\').");
                }

                stream.Consume(PartId.Semicolon);
                break;
            case PartId.Bang:
                Children.Add(Parse(stream));
                break;
            case PartId.Percentage:
                stream.Read();

                var filepath = (string)stream.Consume(PartId.Literal).Value.Expect(TypeId.String);

                var ext = Parse(filepath);

                ext.ReadSubProperties(stream, true);
                
                Children.Add(ext);
                break;
            default:
                throw new ParserException(stream.Line, $"Expected start of child tag or property value, got {head.Type}.");
            }
        }
        
        stream.Consume(PartId.CloseBrace);
    }

    public Part Create()
    {
        var part = Part.Create(PartType);

        foreach ((var name, Obj value) in Properties)
        {
            if (!part.Properties.TryGetValue(name, out var property))
            {
                Logger.Error($"Part {part.PartType} does not contain a property named {name}.");
                continue;
            }

            property[part] = value;
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
        PartId.Key => ParseEnum<Key>(stream),
        PartId.Color => ParseEnum<SCEColor>(stream),
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

    private static Number ParseEnum<TEnum>(PartStream stream)
        where TEnum : struct, IConvertible
    {
        stream.Read();
        stream.Consume(PartId.Period);
        
        var name = stream.Consume(PartId.Identifier).Lexeme;

        if (!Enum.TryParse<TEnum>(name, true, out var key))
        {
            throw new ParserException(stream.Line, $"Unrecognised {typeof(TEnum).Name} {name}.");
        }

        return key.ToInt32(null);
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