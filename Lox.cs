public static class Lox
{
    private static readonly Interpreter interpreter = new Interpreter();
    private static bool hadError = false;
    private static bool hadRuntimeError = false;

    public static void RunFile(string path)
    {
        var source = File.ReadAllText(path);

        Run(source);

        if (hadError)
        {
            System.Environment.Exit(65);
        }

        if (hadRuntimeError)
            System.Environment.Exit(70);

    }

    public static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();

            if (line == null)
            {
                return;
            }

            Run(line);

            hadError = false;
        }
    }

    public static void Error(int line, string message)
    {
        Report(line, "", message);
    }

    public static void Error(Token token, string message)
    {
        if (token.Type == TokenType.EOF)
        {
            Report(token.Line, " at end", message);
        }
        else
        {
            Report(token.Line, $" at '{token.Lexeme}'", message);
        }
    }


    private static void Run(string source)
    {
        var scanner = new Scanner(source);
        var tokens = scanner.ScanTokens();

        // foreach (var token in tokens)
        // {
        //     Console.WriteLine(token.ToString());
        // }

        var parser = new Parser(tokens);
        var statements = parser.Parse();

        if (hadError)
            return;

        var resolver = new Resolver(interpreter);
        resolver.Resolve(statements);

        if (hadError)
            return;

        interpreter.Interpret(statements!);
    }

    private static void Report(int line, string where, string message)
    {
        Console.WriteLine($"[line {line.ToString()}] Error{where}: {message}");

        hadError = true;
    }

    internal static void RunTimeError(RuntimeError error)
    {
        Console.WriteLine($"{error.Message}{System.Environment.NewLine}[line {error.Token.Line}]");
        hadRuntimeError = true;
    }
}
