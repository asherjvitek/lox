using System.Text;

public class AstPrinter : Expr.Visitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this) ?? string.Empty;
    }

    public string VisitBinaryExpr(Expr.Binary expr)
    {
        return Parenthesize(expr.Oper.Lexeme, expr.Left, expr.Right);
    }

    public string VisitGroupingExpr(Expr.Grouping expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    public string VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value?.ToString() ?? "nil";
    }

    public string VisitUnaryExpr(Expr.Unary expr)
    {
        return Parenthesize(expr.Oper.Lexeme, expr.Right);
    }

    public string Parenthesize(string name, params Expr[] exprs)
    {
        var sb = new StringBuilder();

        sb.Append("(").Append(name);
        foreach (var expr in exprs)
        {
            sb.Append($" ");
            sb.Append(expr.Accept(this));
        }
        sb.Append(")");

        return sb.ToString();
    }

    public string VisitVariableExpr(Expr.Variable expr)
    {
        throw new NotImplementedException();
    }

    public string? VisitAssignExpr(Expr.Assign expr)
    {
        throw new NotImplementedException();
    }

    public string? VisitTerneryExpr(Expr.Ternary expr)
    {
        throw new NotImplementedException();
    }

    public string VisitLogicalExpr(Expr.Logical expr)
    {
        throw new NotImplementedException();
    }
}

public class AstRpnPrinter : Expr.Visitor<string>
{
    public string Print(Expr expr)
    {
        return expr.Accept(this) ?? string.Empty;
    }

    public string VisitBinaryExpr(Expr.Binary expr)
    {
        return Parenthesize(expr.Oper.Lexeme, expr.Left, expr.Right);
    }

    public string VisitGroupingExpr(Expr.Grouping expr)
    {
        return Parenthesize("group", expr.Expression);
    }

    public string VisitLiteralExpr(Expr.Literal expr)
    {
        return expr.Value?.ToString() ?? "nil";
    }

    public string VisitUnaryExpr(Expr.Unary expr)
    {
        return Parenthesize(expr.Oper.Lexeme, expr.Right);
    }

    public string Parenthesize(string name, params Expr[] exprs)
    {
        var sb = new StringBuilder();

        sb.Append("(");
        foreach (var expr in exprs)
        {
            sb.Append(expr.Accept(this));
            sb.Append($" ");
        }
        sb.Append(name).Append(")");

        return sb.ToString();
    }

    public string VisitVariableExpr(Expr.Variable expr)
    {
        throw new NotImplementedException();
    }

    public string? VisitAssignExpr(Expr.Assign expr)
    {
        throw new NotImplementedException();
    }

    public string? VisitTerneryExpr(Expr.Ternary expr)
    {
        throw new NotImplementedException();
    }

    public string VisitLogicalExpr(Expr.Logical expr)
    {
        throw new NotImplementedException();
    }
}
