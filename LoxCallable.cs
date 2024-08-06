public interface LoxCallable
{
    object? Call(Interpreter interpreter, List<object?> arguments);
    int Arity();
}

public class Clock : LoxCallable
{
    public int Arity()
    {
        return 0;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        return (double)DateTime.Now.Millisecond / 1000.0d;
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}
