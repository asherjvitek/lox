
public class LoxClass : LoxCallable
{
    public readonly string Name;
    public readonly LoxClass? Superclass;
    public readonly Dictionary<string, LoxFunction> Methods;

    public LoxClass(string name, LoxClass? superclass, Dictionary<string, LoxFunction> methods)
    {
        Name = name;
        Superclass = superclass;
        Methods = methods;
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

        if (Superclass != null)
            return Superclass.FindMethod(name);

        return null;
    }

    public override string ToString()
    {
        return Name;
    }
}
