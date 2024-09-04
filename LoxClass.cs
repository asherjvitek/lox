public class LoxClass : LoxCallable
{
    public readonly string Name;
    public readonly Dictionary<string, LoxFunction> Methods;
    public readonly Dictionary<string, LoxFunction> StaticMethods;

    public LoxClass(string name, Dictionary<string, LoxFunction> methods, Dictionary<string, LoxFunction> staticMethods)
    {
        Name = name;
        Methods = methods;
        StaticMethods = staticMethods;
    }

    public int Arity()
    {
        var initializer = FindMethod("init");

        if (initializer == null)
            return 0;

        return initializer.Arity();
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var instance = new LoxInstance(this);

        var initializer = FindMethod("init");

        if (initializer != null)
            initializer.Bind(instance).Call(interpreter, arguments);

        return instance;
    }

    public LoxFunction? FindMethod(string name)
    {
        if (Methods.TryGetValue(name, out var method))
            return method;

        return null;
    }

    public object? Get(Token name)
    {
        if (StaticMethods.TryGetValue(name.Lexeme, out var method))
            return method;

        throw new RuntimeError(name, $"Undefined static method '{name.Lexeme}'.");
    }

    public override string ToString()
    {
        return Name;
    }
}
