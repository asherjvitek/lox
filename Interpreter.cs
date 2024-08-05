using static TokenType;

public class Interpreter : Expr.Visitor<object>, Stmt.Visitor
{
    private Environment environment = new Environment(null);

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var statement in statements)
            {
                Execute(statement);
            }
        }
        catch (RuntimeError error)
        {
            Lox.RunTimeError(error);
        }
    }

    private void Execute(Stmt statement)
    {
        statement.Accept(this);
    }

    private void ExecuteBlock(List<Stmt> statements, Environment environment)
    {
        var previous = this.environment;

        try
        {
            this.environment = environment;

            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
        }
        finally
        {
            this.environment = previous;
        }
    }

    private string Stringify(object? value)
    {
        if (value == null) return "nil";

        if (value is double d)
        {
            var text = d.ToString();

            if (text.EndsWith(".0"))
            {
                text = text.Substring(0, text.Length - 2);
            }

            return text;
        }

        return value.ToString()!;
    }

    public object VisitBinaryExpr(Expr.Binary expr)
    {
        var left = Evaluate(expr.Left);
        var right = Evaluate(expr.Right);

        switch (expr.Oper.Type)
        {
            case BANG_EQUAL:
                return !IsEqual(left, right);
            case EQUAL_EQUAL:
                return IsEqual(left, right);
            case GREATER:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! > (double)right!;
            case GREATER_EQUAL:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! >= (double)right!;
            case LESS:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! < (double)right!;
            case LESS_EQUAL:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! <= (double)right!;
            case MINUS:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! - (double)right!;
            case SLASH:
                CheckNumberOperands(expr.Oper, left, right);
                CheckDivideBy0(expr.Oper, right);
                return (double)left! / (double)right!;
            case STAR:
                CheckNumberOperands(expr.Oper, left, right);
                return (double)left! * (double)right!;
            case PLUS:
                if (left is double ld && right is double rd)
                {
                    return ld + rd;
                }

                if (left is string ls && right is string rs)
                {
                    return ls + rs;
                }

                return left?.ToString() + right?.ToString();
            default:
                break;
        }

        throw new RuntimeError(expr.Oper, "Operands must be two numbers or two strings");
    }

    public object? VisitTerneryExpr(Expr.Ternary ternary)
    {
        if (IsTruthy(Evaluate(ternary.Condition)))
        {
            return Evaluate(ternary.ThenExpression);
        }

        return Evaluate(ternary.ElseExpression);
    }

    private void CheckDivideBy0(Token oper, object? right)
    {
        if (double.TryParse(right?.ToString(), out double value) && value == 0d)
            throw new RuntimeError(oper, "Divide by 0 Error");
    }

    private bool IsEqual(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null) return false;

        return left.Equals(right);
    }

    public object VisitGroupingExpr(Expr.Grouping expr)
    {
        return Evaluate(expr)!;
    }

    public object? VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value;
    }

    public object VisitUnaryExpr(Expr.Unary expr)
    {
        var right = Evaluate(expr.Right);

        switch (expr.Oper.Type)
        {
            case BANG:
                return !IsTruthy(right);
            case MINUS:
                CheckNumberOperand(expr.Oper, right);
                return -(double)right!;
            default:
                break;
        }

        throw new Exception("should not be hittable? But this totally is at the moment");
    }

    public object? VisitVariableExpr(Expr.Variable expr)
    {
        return environment.Get(expr.Name);
    }

    public object? VisitAssignExpr(Expr.Assign expr)
    {
        var value = Evaluate(expr.Value);

        environment.Assign(expr.Name, value);

        return value;
    }

    public object? VisitLogicalExpr(Expr.Logical expr)
    {
        var left = Evaluate(expr.Left);

        if (expr.Oper.Type == TokenType.OR)
        {
            if (IsTruthy(left))
                return left;
        }
        else
        {
            if (!IsTruthy(left))
                return left;
        }

        return Evaluate(expr.Right);
    }

    public void VisitExpressionStmt(Stmt.Expression stmt)
    {
        Evaluate(stmt.Expr);
    }

    public void VisitIfStmt(Stmt.If stmt)
    {
        if (IsTruthy(Evaluate(stmt.Condition)))
        {
            Execute(stmt.ThenBranch);
        }
        else if (stmt.ElseBranch != null)
        {
            Execute(stmt.ElseBranch);
        }
    }


    public void VisitPrintStmt(Stmt.Print stmt)
    {
        Console.WriteLine(Evaluate(stmt.Expr));
    }

    public void VisitVarStmt(Stmt.Var stmt)
    {
        object? value = new UnassignedVariable();
        if (stmt.Initializer != null)
        {
            value = Evaluate(stmt.Initializer);
        }

        environment.Define(stmt.Name.Lexeme, value);
    }

    public void VisitBlockStmt(Stmt.Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Environment(environment));
    }

    private bool IsTruthy(object? right)
    {
        if (right == null) return false;
        if (right is bool res) return res;
        return true;
    }

    private object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    private void CheckNumberOperands(Token oper, object? left, object? right)
    {
        if (left is not double || right is not double)
            throw new RuntimeError(oper, "Operand must be a number.");
    }

    private void CheckNumberOperand(Token oper, object? operand)
    {
        if (operand is not double)
            throw new RuntimeError(oper, "Operand must be a number.");
    }

    public void VisitWhileStmt(Stmt.While stmt)
    {
        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            try
            {
                Execute(stmt.Body);
            }
            catch (Break)
            {
                break;
            }
            catch (Continue)
            {
                continue;
            }
        }
    }

    public void VisitForStmt(Stmt.For stmt)
    {
        if (stmt.Initializer != null)
            Execute(stmt.Initializer);

        while (IsTruthy(Evaluate(stmt.Condition)))
        {
            try
            {
                Execute(stmt.Body);
            }
            catch (Break)
            {
                break;
            }
            catch (Continue)
            {
                if (stmt.Incrementer != null)
                    Evaluate(stmt.Incrementer);

                continue;
            }

            if (stmt.Incrementer != null)
                Evaluate(stmt.Incrementer);
        }

    }

    public void VisitBreakStmt(Stmt.Break stmt)
    {
        throw new Break();
    }

    public void VisitContinueStmt(Stmt.Continue stmt)
    {
        throw new Continue();
    }
}
