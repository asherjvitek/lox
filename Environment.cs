public class Environment
{
    public readonly Dictionary<string, object?> Values = new Dictionary<string, object?>();

    public readonly Environment? Enclosing;

    public Environment()
    {
        Enclosing = null;
    }

    public Environment(Environment? enclosing)
    {
        Enclosing = enclosing;
    }

    public void Define(string name, object? value)
    {
        Values[name] = value;
    }

    public void Assign(Token name, object? value)
    {
        if (Values.ContainsKey(name.Lexeme))
        {
            Values[name.Lexeme] = value;

            return;
        }

        if (Enclosing != null)
        {
            Enclosing.Assign(name, value);

            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}");
    }

    internal void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance)!.Values[name.Lexeme] = value;
    }

    public object? Get(Token name)
    {
        var exists = Values.TryGetValue(name.Lexeme, out object? value);

        if (value is UnassignedVariable)
            throw new RuntimeError(name, $"Unassigned variable '{name.Lexeme}");

        if (exists)
            return value;

        if (Enclosing != null)
            return Enclosing.Get(name);

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public object? GetAt(int distance, string name)
    {
        try
        {
            return Ancestor(distance)?.Values[name];
        }
        catch (System.Exception)
        {
            Console.WriteLine($"distance: {distance}; name: {name}");
            throw;
        }
    }

    private Environment? Ancestor(int distance)
    {
        Environment? environment = this;

        for (int i = 0; i < distance; i++)
        {
            environment = environment?.Enclosing;
        }

        return environment;
    }
}
