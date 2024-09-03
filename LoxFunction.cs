public class LoxFunction : LoxCallable
{
    private readonly Stmt.Function declaration;
    private readonly Environment closure;
    private readonly bool isInitializer = false;

    public LoxFunction(Stmt.Function declaration, Environment closure, bool isInitializer)
    {
        this.declaration = declaration;
        this.closure = closure;
        this.isInitializer = isInitializer;
    }

    public int Arity()
    {
        return declaration.Params.Count;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var environment = new Environment(closure);

        for (int i = 0; i < declaration.Params.Count; i++)
        {
            environment.Define(declaration.Params[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment);
        }
        catch (Return exp)
        {
            if (isInitializer)
                return closure.GetAt(0, "this");

            return exp.Value;
        }

        if (isInitializer)
        {
            return closure.GetAt(0, "this");
        }

        return null;
    }

    public LoxFunction Bind(LoxInstance instance)
    {
        var environment = new Environment(closure);

        environment.Define("this", instance);

        return new LoxFunction(declaration, environment, isInitializer);
    }

    public override string ToString()
    {
        return $"<fn {declaration.Name.Lexeme}>";
    }
}
