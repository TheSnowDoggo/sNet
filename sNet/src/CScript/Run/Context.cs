namespace sNet.CScriptPro;

public sealed class Context
{
    private readonly Dictionary<string, Stack<Variable>> _localVariables = [];
    private readonly Stack<HashSet<string>> _scopes = [];
    
    public Context(Context parent = null)
    {
        Parent = parent;
    }

    public int Line { get; set; } = 1;
    public Context Parent { get; }

    public CObj this[string name]
    {
        get
        {
            if (TryGetLocal(name, out var variable))
            {
                return variable.Value;
            }

            if (Global.Exports.TryGetValue(name, out var global))
            {
                return global;
            }

            throw new InterpreterException(Line, $"Local or global variable (\'{name}\') is undefined in the current scope.");
        }
        set
        {
            if (!TryGetLocal(name, out var variable))
            {
                throw new InterpreterException(Line, $"Local or global variable (\'{name}\') is undefined in the current scope.");
            }

            if ((variable.Attributes & VariableAttribute.Const) != 0)
            {
                throw new InterpreterException(Line, $"Cannot assign constant variable (\'{name}\').");
            }
            
            variable.Value = value;
        }
    }

    public void CreateScope()
    {
        _scopes.Push([]);
    }

    public void CloseScope()
    {
        if (!_scopes.TryPop(out var scope))
        {
            throw new InterpreterException(Line, "Cannot close scope as no scopes are currently defined.");
        }

        foreach (var name in scope)
        {
            if (!_localVariables.TryGetValue(name, out var variables))
            {
                throw new InterpreterException(Line, $"Failed to find variable (\'{name}\').");
            }

            if (variables.Count == 0)
            {
                throw new InterpreterException(Line, "Scopes variables was empty?");
            }

            variables.Pop();

            if (variables.Count == 0)
            {
                _localVariables.Remove(name);
            }
        }
    }
    
    public void Define(string name, CObj value, VariableAttribute attributes = VariableAttribute.None)
    {
        if (!_scopes.TryPeek(out var scope))
        {
            throw new InterpreterException(Line, "No scopes have been defined.");
        }

        if (!scope.Add(name))
        {
            throw new InterpreterException(Line, $"Variable (\'{name}\') has already been defined in this scope.");
        }
        
        if (!_localVariables.TryGetValue(name, out var variables))
        {
            _localVariables[name] = variables = [];
        }

        variables.Push(new Variable(value, attributes));
    }

    private bool TryGetLocal(string name, out Variable variable)
    {
        var current = this;

        while (current != null)
        {
            if (current._localVariables.TryGetValue(name, out var stack))
            {
                variable = stack.Peek();
                return true;
            }
            
            current = current.Parent;
        }

        variable = null;
        return false;
    }
}