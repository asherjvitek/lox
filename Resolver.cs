public class Resolver : Expr.Visitor, Stmt.Visitor
{
    private readonly Interpreter interpreter;
    private Stack<Dictionary<string, bool>> scopes = new Stack<Dictionary<string, bool>>();
    private FunctionType currentFunction = FunctionType.NONE;
    private ClassType currentClass = ClassType.NONE;

    private enum FunctionType
    {
        NONE,
        FUNCTION,
        INITIALIZER,
        METHOD,
    }

    private enum ClassType
    {
        NONE,
        CLASS,
        SUBCLASS,
    }

    public Resolver(Interpreter interpreter)
    {
        this.interpreter = interpreter;
    }

    public void Resolve(List<Stmt> stmts)
    {
        foreach (var stmt in stmts)
        {
            Resolve(stmt);
        }
    }

    public void VisitVariableExpr(Expr.Variable expr)
    {
        if (scopes.Any() && scopes.Peek().TryGetValue(expr.Name.Lexeme, out bool value) && value == false)
        {
            Lox.Error(expr.Name, "Can't read local variable in its own initializer");
        }

        ResolveLocal(expr, expr.Name);
    }

    public void VisitAssignExpr(Expr.Assign expr)
    {
        Resolve(expr.Value);
        ResolveLocal(expr, expr.Name);
    }

    public void VisitBinaryExpr(Expr.Binary expr)
    {
        Resolve(expr.Right);
        Resolve(expr.Left);
    }

    public void VisitBlockStmt(Stmt.Block stmt)
    {
        BeginScope();
        Resolve(stmt.Statements);
        EndScope();
    }

    public void VisitBreakStmt(Stmt.Break stmt)
    {

    }

    public void VisitCallExpr(Expr.Call expr)
    {
        Resolve(expr.Callee);

        foreach (var argument in expr.Arguments)
        {
            Resolve(argument);
        }
    }

    public void VisitGetExpr(Expr.Get expr)
    {
        Resolve(expr.Object);
    }

    public void VisitSetExpr(Expr.Set expr)
    {
        Resolve(expr.Value);
        Resolve(expr.Object);
    }

    public void VisitSuperExpr(Expr.Super expr)
    {
        if (currentClass == ClassType.NONE)
        {
            Lox.Error(expr.Keyword, "Can't user 'super' outside of a class.");
        }
        else if (currentClass != ClassType.SUBCLASS)
        {
            Lox.Error(expr.Keyword, "Can't user 'super' without a superclass.");
        }

        ResolveLocal(expr, expr.Keyword);
    }

    public void VisitThisExpr(Expr.This expr)
    {
        if (currentClass == ClassType.NONE)
        {
            Lox.Error(expr.Keyword, "Can't user 'this' outside of a class.");

            return;
        }

        ResolveLocal(expr, expr.Keyword);
    }

    public void VisitContinueStmt(Stmt.Continue stmt)
    {

    }

    public void VisitExpressionStmt(Stmt.Expression stmt)
    {
        Resolve(stmt.Expr);
    }

    public void VisitForStmt(Stmt.For stmt)
    {
        if (stmt.Initializer != null)
            Resolve(stmt.Initializer);

        Resolve(stmt.Condition);
        Resolve(stmt.Body);

        if (stmt.Incrementer != null)
            Resolve(stmt.Incrementer);
    }

    public void VisitClassStmt(Stmt.Class stmt)
    {
        var enclosingClass = currentClass;
        currentClass = ClassType.CLASS;

        Declare(stmt.Name);
        Define(stmt.Name);

        if (stmt.Superclass != null && stmt.Name.Lexeme.Equals(stmt.Superclass.Name.Lexeme))
            Lox.Error(stmt.Superclass.Name, "A class can't inherit from itself.");

        if (stmt.Superclass != null)
        {
            currentClass = ClassType.SUBCLASS;
            Resolve(stmt.Superclass);
        }

        if (stmt.Superclass != null)
        {
            BeginScope();
            scopes.Peek()["super"] = true;
        }

        BeginScope();
        scopes.Peek()["this"] = true;

        foreach (var method in stmt.Methods)
        {
            var declaration = FunctionType.METHOD;

            if (method.Name.Lexeme == "init")
                declaration = FunctionType.INITIALIZER;

            ResolveFunction(method, declaration);
        }

        EndScope();

        if (stmt.Superclass != null)
        {
            EndScope();
        }

        currentClass = ClassType.NONE;
    }

    public void VisitFunctionStmt(Stmt.Function stmt)
    {
        Declare(stmt.Name);
        Define(stmt.Name);

        ResolveFunction(stmt, FunctionType.FUNCTION);
    }

    public void VisitGroupingExpr(Expr.Grouping expr)
    {
        Resolve(expr.Expression);
    }

    public void VisitIfStmt(Stmt.If stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.ThenBranch);

        if (stmt.ElseBranch != null)
            Resolve(stmt.ElseBranch);
    }

    public void VisitLambdaExpr(Expr.Lambda expr)
    {
        BeginScope();

        foreach (var param in expr.Parameters)
        {
            Declare(param);
            Define(param);
        }

        Resolve(expr.Body);
        EndScope();
    }

    public void VisitLiteralExpr(Expr.Literal expr)
    {
        //Do Nothing
    }

    public void VisitLogicalExpr(Expr.Logical expr)
    {
        Resolve(expr.Right);
        Resolve(expr.Left);
    }

    public void VisitPrintStmt(Stmt.Print stmt)
    {
        Resolve(stmt.Expr);
    }

    public void VisitReturnStmt(Stmt.Return stmt)
    {
        if (currentFunction == FunctionType.NONE)
            Lox.Error(stmt.Keyword, "Can't return from top-level code.");

        if (stmt.Value != null)
        {
            if (currentFunction == FunctionType.INITIALIZER)
            {
                Lox.Error(stmt.Keyword, "Can't return a value from an initializer.");
            }

            Resolve(stmt.Value);
        }
    }

    public void VisitTerneryExpr(Expr.Ternary expr)
    {
        Resolve(expr.Condition);
        Resolve(expr.ThenExpression);
        Resolve(expr.ElseExpression);
    }

    public void VisitUnaryExpr(Expr.Unary expr)
    {
        Resolve(expr.Right);
    }

    public void VisitVarStmt(Stmt.Var stmt)
    {
        Declare(stmt.Name);

        if (stmt.Initializer != null)
        {
            Resolve(stmt.Initializer);
        }

        Define(stmt.Name);
    }

    public void VisitWhileStmt(Stmt.While stmt)
    {
        Resolve(stmt.Condition);
        Resolve(stmt.Body);
    }

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void Resolve(Expr expr)
    {
        expr.Accept(this);
    }

    private void BeginScope()
    {
        scopes.Push(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        scopes.Pop();
    }

    private void Declare(Token name)
    {
        if (!scopes.Any()) return;

        var scope = scopes.Peek();

        if (scope.ContainsKey(name.Lexeme))
        {
            Lox.Error(name, "Already a variable with this name in this scope.");
        }

        scope[name.Lexeme] = false;
    }

    private void Define(Token name)
    {
        if (!scopes.Any()) return;

        scopes.Peek()[name.Lexeme] = true;
    }

    private void ResolveLocal(Expr expr, Token name)
    {
        for (int i = 0; i < scopes.Count; i++)
        {
            if (scopes.ElementAt(i).ContainsKey(name.Lexeme))
            {
                interpreter.Resolve(expr, i);
                return;
            }
        }
    }

    private void ResolveFunction(Stmt.Function function, FunctionType type)
    {
        var enclosingFunction = currentFunction;
        currentFunction = type;

        BeginScope();

        foreach (var param in function.Params)
        {
            Declare(param);
            Define(param);
        }

        Resolve(function.Body);
        EndScope();

        currentFunction = enclosingFunction;
    }
}
