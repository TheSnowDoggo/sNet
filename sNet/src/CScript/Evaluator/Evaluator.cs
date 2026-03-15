using System.Collections.Frozen;
using System.Runtime.CompilerServices;

namespace sNet.CScriptPro;

public sealed class Evaluator
{
    private static readonly FrozenDictionary<CsrId, FrozenDictionary<TypeSet, Binary>> BinaryOperators = new Dictionary<CsrId, FrozenDictionary<TypeSet, Binary>>
    {
        { CsrId.Mul, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x * (Number)y },
            { (TypeId.Vec2, TypeId.Vec2), (x, y) => (Vec2Obj)x * (Vec2Obj)y },
            { (TypeId.Vec2, TypeId.Number), (x, y) => (Vec2Obj)x * (Number)y },
            { (TypeId.Number, TypeId.Vec2), (x, y) => (Vec2Obj)y * (Number)x },
        }.ToFrozenDictionary() },
        
        { CsrId.Div, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x / (Number)y },
            { (TypeId.Vec2, TypeId.Vec2), (x, y) => (Vec2Obj)x / (Vec2Obj)y },
            { (TypeId.Vec2, TypeId.Number), (x, y) => (Vec2Obj)x / (Number)y },
            { (TypeId.Number, TypeId.Vec2), (x, y) => (Vec2Obj)y / (Number)x },
        }.ToFrozenDictionary() },
        
        { CsrId.Rem, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x % (Number)y },
        }.ToFrozenDictionary() },
        
        { CsrId.Add, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.String, TypeId.String), (x, y) => (StrObj)x + (StrObj)y },
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x + (Number)y },
            { (TypeId.Vec2, TypeId.Vec2), (x, y) => (Vec2Obj)x + (Vec2Obj)y },
        }.ToFrozenDictionary() },
        
        { CsrId.Sub, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x - (Number)y },
            { (TypeId.Vec2, TypeId.Vec2), (x, y) => (Vec2Obj)x - (Vec2Obj)y },
        }.ToFrozenDictionary() },
        
        { CsrId.ShiftLeft, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x << (Number)y },
        }.ToFrozenDictionary() },
        
        { CsrId.ShiftRight, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x >> (Number)y },
        }.ToFrozenDictionary() },

        { CsrId.LessThan, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.String, TypeId.String), (x, y) => (StrObj)x < (StrObj)y },
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x < (Number)y },
        }.ToFrozenDictionary() },
        
        { CsrId.GreaterThan, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.String, TypeId.String), (x, y) => (StrObj)x > (StrObj)y },
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x > (Number)y },
        }.ToFrozenDictionary() },
        
        { CsrId.LessThanOrEqual, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.String, TypeId.String), (x, y) => (StrObj)x <= (StrObj)y },
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x <= (Number)y },
        }.ToFrozenDictionary() },
        
        { CsrId.GreaterThanOrEqual, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.String, TypeId.String), (x, y) => (StrObj)x >= (StrObj)y },
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x >= (Number)y },
        }.ToFrozenDictionary() },

        { CsrId.And, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.Bool, TypeId.Bool), (x, y) => (Bool)x & (Bool)y },
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x & (Number)y },
        }.ToFrozenDictionary() },
        
        { CsrId.Xor, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.Bool, TypeId.Bool), (x, y) => (Bool)x ^ (Bool)y },
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x ^ (Number)y },
        }.ToFrozenDictionary() },
        
        { CsrId.Or, new Dictionary<TypeSet, Binary>
        {
            { (TypeId.Bool, TypeId.Bool), (x, y) => (Bool)x | (Bool)y },
            { (TypeId.Number, TypeId.Number), (x, y) => (Number)x | (Number)y },
        }.ToFrozenDictionary() },
    }.ToFrozenDictionary();
    
    private readonly Context _context;
    private readonly List<CsrToken> _tokens;
    private Stack<Obj> _stack;

    public Evaluator(Context context, List<CsrToken> tokens)
    {
        _context = context;
        _tokens = tokens;
    }

    public Obj Evaluate()
    {
        _stack = [];
        
        foreach (var token in _tokens)
        {
            NextToken(token);
        }
        
        var stack = _stack;
        _stack = null;

        if (stack.Count == 0)
        {
            return Nil.Value;
        }

        if (stack.Count > 1)
        {
            throw new InterpreterException(_context.Line, "Failed to evaluate expression: Stack contained multiple values.");
        }

        var value = stack.Pop();
        Obj.Deref(ref value);
        return value;
    }

    private void NextToken(CsrToken csrToken)
    {
        switch (csrToken.Type)
        {
        case CsrId.Literal:
            _stack.Push(csrToken.Value.Clone());
            break;
        case CsrId.Identifier:
            _stack.Push(new VariableRefObj(csrToken.Lexeme, _context));
            break;
        case CsrId.Equals:
            EvalEquals();
            break;
        case CsrId.NotEquals:
            EvalNotEquals();
            break;
        case CsrId.Assign:
            EvalAssign();
            break;
        case CsrId.Complement:
            EvalComplement();
            break;
        case CsrId.Not:
            EvalNot();
            break;
        case CsrId.Minus:
            EvalMinus();
            break;
        case CsrId.Invoke:
            EvalInvoke(csrToken);
            break;
        case CsrId.Function:
            EvalFunction(csrToken);
            break;
        case CsrId.ArrayDefinition:
            EvalArrayDefintion(csrToken);
            break;
        case CsrId.TableDefinition:
            EvalTableDefintion(csrToken);
            break;
        case CsrId.Period:
            EvalMemberRef(csrToken);
            break;
        case CsrId.Cast:
            EvalCast(csrToken);
            break;
        case CsrId.Typeof:
            EvalTypeof();
            break;
        case CsrId.DynamicCast:
            EvalDynamicCast();
            break;
        default:
            if (csrToken.IsCompound())
            {
                EvalBinaryCompound(csrToken);
            }
            else
            {
                EvalBinary(csrToken);
            }
            break;
        }
    }

    private void PopUnary(out Obj value)
    {
        if (!_stack.TryPop(out value))
        {
            throw new InterpreterException(_context.Line, $"Unary operator expected one operand, got {_stack.Count}.");
        }
    }
    
    private void PopUnaryDeref(out Obj value)
    {
        PopUnary(out value);
        Obj.Deref(ref value);
    }

    private void PopBinary(out Obj left, out Obj right)
    {
        if (_stack.Count < 2)
        {
            throw new InterpreterException(_context.Line, $"Binary operator expected two operands, got {_stack.Count}.");
        }
        
        right = _stack.Pop();
        left = _stack.Pop();
    }
    
    private void PopBinaryDeref(out Obj left, out Obj right)
    {
        PopBinary(out left, out right);
        Obj.Deref(ref left);
        Obj.Deref(ref right);
    }
    
    private void EvalEquals()
    {
        PopBinaryDeref(out var left, out var right);
        
        _stack.Push((Bool)left.Equals(right));
    }
    
    private void EvalNotEquals()
    {
        PopBinaryDeref(out var left, out var right);
        
        _stack.Push((Bool)!left.Equals(right));
    }
    
    private void EvalAssign()
    {
        PopBinary(out var left, out var right);
        Obj.Deref(ref right);
        
        if (left is not RefObj cref)
        {
            throw new InterpreterException(_context.Line, $"Cannot assign to non-reference type {left.TypeId}.");
        }

        cref.Value = right;
    }

    private void EvalComplement()
    {
        PopUnaryDeref(out var value);
        ThrowIfNot(value, TypeId.Number);
        _stack.Push(~(Number)value);
    }

    private void EvalNot()
    {
        PopUnaryDeref(out var value);
        ThrowIfNot(value, TypeId.Bool);
        _stack.Push(!(Bool)value);
    }

    private void EvalMinus()
    {
        PopUnaryDeref(out var x);
        ThrowIfNot(x, TypeId.Number);
        _stack.Push(-(Number)x);
    }

    private void EvalInvoke(CsrToken csrToken)
    {
        int argCount = (int)(Number)csrToken.Value;

        if (_stack.Count < argCount)
        {
            throw new InterpreterException(_stack.Count, $"Function expected {argCount} args, stack only contained {_stack.Count}.");
        }
        
        var args = new Obj[argCount];
        
        for (int i = args.Length - 1; i >= 0; i--)
        {
            args[i] = _stack.Pop();
            Obj.Deref(ref args[i]);
        }
        
        PopUnaryDeref(out var function);

        if (function.TypeId != TypeId.Function)
        {
            throw new InterpreterException(_context.Line, $"Cannot invoke non-invokable type {function.TypeId}.");
        }

        _stack.Push(((Function)function).Run(args));
    }

    private void EvalFunction(CsrToken csrToken)
    {
        var definition = (FunctionDefinition)csrToken.Value;
        
        _stack.Push(definition.Create(_context));
    }

    private void EvalArrayDefintion(CsrToken csrToken)
    {
        var definition = (ArrayDef)csrToken.Value;
        
        _stack.Push(definition.Create(_context));
    }
    
    private void EvalTableDefintion(CsrToken csrToken)
    {
        var definition = (TableDefinition)csrToken.Value;
        
        _stack.Push(definition.Create(_context));
    }

    private void EvalMemberRef(CsrToken csrToken)
    {
        PopBinary(out var left, out var right);
        Obj.Deref(ref left);
        
        if (!ReferenceEquals(csrToken.Value, Bool.True) && right is VariableRefObj vref)
        {
            right = (StrObj)vref.Name;
        }
        else
        {
            Obj.Deref(ref right);
        }
        
        _stack.Push(new MemberRefObj(left, right));
    }

    private void EvalCast(CsrToken csrToken)
    {
        PopUnaryDeref(out var x);
        
        var cType = (TypeObj)csrToken.Value;
        
        _stack.Push(x.Cast(cType.Id));
    }

    private void EvalTypeof()
    {
        PopUnaryDeref(out var x);
        _stack.Push(x.TypeId.ToString());
    }
    
    private void EvalDynamicCast()
    {
        PopBinaryDeref(out var left, out var right);
        ThrowIfNot(right, TypeId.String);

        if (!Enum.TryParse<TypeId>((string)(StrObj)right, true, out var type))
        {
            throw new InterpreterException(_context.Line, $"Unrecognised cast type {right}.");
        }
        
        _stack.Push(left.Cast(type));
    }

    private void EvalBinary(CsrToken op)
    {
        if (!BinaryOperators.TryGetValue(op.Type, out var operatorSet))
        {
            throw new InterpreterException(op.Line, $"No binary operator exists for operator {op.Type}.");
        }
        
        PopBinaryDeref(out var left, out var right);
        
        if (!operatorSet.TryGetValue(new TypeSet(left.TypeId, right.TypeId), out var operation))
        {
            throw new InterpreterException(op.Line, $"No binary operator for {op.Type} exists between {left} and {right}.");
        }
        
        _stack.Push(operation.Invoke(left, right));
    }

    private void EvalBinaryCompound(CsrToken op)
    {
        if (!BinaryOperators.TryGetValue(op.Type, out var operatorSet))
        {
            throw new InterpreterException(op.Line, $"No compound binary operator exists for operator {op.Type}.");
        }
        
        PopBinary(out var left, out var right);
        Obj.Deref(ref right);

        if (left is not RefObj cref)
        {
            throw new InterpreterException(_context.Line, $"Cannot assign to non-reference type {left.TypeId}.");
        }
        
        left = cref.Value;
        
        if (!operatorSet.TryGetValue(new TypeSet(left.TypeId, right.TypeId), out var operation))
        {
            throw new InterpreterException(op.Line, $"No binary operator for {op.Type} exists between {left} and {right}.");
        }
        
        _stack.Push(cref.Value = operation.Invoke(left, right));
    }

    private void ThrowIfNot(Obj obj, TypeId expectedTypeId, [CallerMemberName] string memberName = "")
    {
        if (obj.TypeId != expectedTypeId)
        {
            throw new InterpreterException(_context.Line, $"At {memberName}(): Expected type {expectedTypeId}, got {obj.TypeId}.");
        }
    }
}