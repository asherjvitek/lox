public class LoxInstance
{
    private static Dictionary<string, object?> fields = new Dictionary<string, object?>();

    public LoxClass LoxClass;

    public LoxInstance(LoxClass loxClass)
    {
        LoxClass = loxClass;
    }

    public object? Get(Token name)
    {
        if (fields.TryGetValue(name.Lexeme, out var value))
        {
            return value;
        }

        var method = LoxClass.FindMethod(name.Lexeme);
        if (method != null)
            return method.Bind(this);

        throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
    }

    public void Set(Token name, object? value)
    {
        fields[name.Lexeme] = value;
    }

    public override string ToString()
    {
        return $"{LoxClass.Name} instance";
    }
}
