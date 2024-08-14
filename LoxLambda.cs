public class LoxLambda : LoxCallable
{
    private readonly Expr.Lambda declaration;
    private readonly Environment closure;

    public LoxLambda(Expr.Lambda declaration, Environment closure)
    {
        this.declaration = declaration;
        this.closure = closure;
    }

    public int Arity()
    {
        return declaration.Parameters.Count;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var environment = new Environment(closure);

        for (int i = 0; i < declaration.Parameters.Count; i++)
        {
            environment.Define(declaration.Parameters[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment);
        }
        catch (Return exp)
        {
            return exp.Value;
        }

        return null;
    }

    public override string ToString()
    {
        return $"<fn Anonamous Function>";
    }
}

