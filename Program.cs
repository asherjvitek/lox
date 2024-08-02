if (args.Length > 1)
{
    Console.WriteLine("Usage: jlox [script]");
    System.Environment.Exit(0);
}
else if (args.Length == 1)
{
    Lox.RunFile(args[0]);
}
else
{
    Lox.RunPrompt();
}
