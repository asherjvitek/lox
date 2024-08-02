public class Environment
{
    private readonly Dictionary<string, object?> values = new Dictionary<string, object?>();

    public readonly Environment? Enclosing;

    public Environment(Environment? enclosing)
    {
        Enclosing = enclosing;
    }

    public void Define(string name, object? value)
    {
        values[name] = value;
    }

    public void Assign(Token name, object? value)
    {
        if (values.ContainsKey(name.Lexeme))
        {
            values[name.Lexeme] = value;

            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);

            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}");
    }

    public object? Get(Token name)
    {
        var exists = values.TryGetValue(name.Lexeme, out object? value);

        if (value is UnassignedVariable)
            throw new RuntimeError(name, $"Unassigned variable '{name.Lexeme}");

        if (exists)
            return value;

        if (Enclosing != null)
            return Enclosing.Get(name);

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }
}
