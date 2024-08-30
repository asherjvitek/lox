public abstract class Expr
{
    public abstract T? Accept<T>(Visitor<T> visitor);
    public abstract void Accept(Visitor visitor);

    public interface Visitor<T>
    {
        T VisitBinaryExpr(Binary expr);
        T VisitGroupingExpr(Grouping expr);
        T? VisitLiteralExpr(Literal expr);
        T VisitUnaryExpr(Unary expr);
        T? VisitVariableExpr(Variable expr);
        T? VisitAssignExpr(Assign expr);
        T? VisitTerneryExpr(Ternary expr);
        T? VisitLogicalExpr(Logical expr);
        T? VisitCallExpr(Call expr);
        T? VisitLambdaExpr(Lambda expr);
    }

    public interface Visitor
    {
        void VisitBinaryExpr(Binary expr);
        void VisitGroupingExpr(Grouping expr);
        void VisitLiteralExpr(Literal expr);
        void VisitUnaryExpr(Unary expr);
        void VisitVariableExpr(Variable expr);
        void VisitAssignExpr(Assign expr);
        void VisitTerneryExpr(Ternary expr);
        void VisitLogicalExpr(Logical expr);
        void VisitCallExpr(Call expr);
        void VisitLambdaExpr(Lambda expr);
    }

    public class Binary : Expr
    {
        public readonly Expr Left;
        public readonly Token Oper;
        public readonly Expr Right;

        public Binary(Expr left, Token oper, Expr right)
        {
            this.Left = left;
            this.Oper = oper;
            this.Right = right;
        }

        public override T Accept<T>(Visitor<T> visitor) => visitor.VisitBinaryExpr(this);
        public override void Accept(Visitor visitor) => visitor.VisitBinaryExpr(this);

    }

    public class Lambda : Expr
    {
        public readonly List<Token> Parameters;
        public readonly List<Stmt> Body;


        public Lambda(List<Token> parameters, List<Stmt> body)
        {
            Parameters = parameters;
            Body = body;
        }

        public override T? Accept<T>(Visitor<T> visitor) where T : default => visitor.VisitLambdaExpr(this);
        public override void Accept(Visitor visitor)  => visitor.VisitLambdaExpr(this);
    }

    public class Call : Expr
    {
        public readonly Expr Callee;
        public readonly Token Paren;
        public readonly List<Expr> Arguments;

        public Call(Expr callee, Token paren, List<Expr> arguments)
        {
            Callee = callee;
            Paren = paren;
            Arguments = arguments;
        }

        public override T? Accept<T>(Visitor<T> visitor) where T : default => visitor.VisitCallExpr(this);
        public override void Accept(Visitor visitor)  => visitor.VisitCallExpr(this);
    }

    public class Logical : Expr
    {
        public readonly Expr Left;
        public readonly Token Oper;
        public readonly Expr Right;

        public Logical(Expr left, Token oper, Expr right)
        {
            this.Left = left;
            this.Oper = oper;
            this.Right = right;
        }

        public override T? Accept<T>(Visitor<T> visitor) where T : default => visitor.VisitLogicalExpr(this);
        public override void Accept(Visitor visitor)  => visitor.VisitLogicalExpr(this);

    }

    public class Ternary : Expr
    {
        public readonly Expr Condition;
        public readonly Expr ThenExpression;
        public readonly Expr ElseExpression;

        public Ternary(Expr condition, Expr thenExpression, Expr elseExpression)
        {
            Condition = condition;
            ThenExpression = thenExpression;
            ElseExpression = elseExpression;
        }

        public override T? Accept<T>(Visitor<T> visitor) where T : default => visitor.VisitTerneryExpr(this);
        public override void Accept(Visitor visitor) => visitor.VisitTerneryExpr(this);
    }

    public class Grouping : Expr
    {
        public readonly Expr Expression;

        public Grouping(Expr expression)
        {
            this.Expression = expression;
        }

        public override T Accept<T>(Visitor<T> visitor) => visitor.VisitGroupingExpr(this);
        public override void Accept(Visitor visitor) => visitor.VisitGroupingExpr(this);

    }

    public class Literal : Expr
    {
        public readonly object? Value;

        public Literal(object? value)
        {
            this.Value = value;
        }

        public override T? Accept<T>(Visitor<T> visitor) where T : default => visitor.VisitLiteralExpr(this);
        public override void Accept(Visitor visitor)  => visitor.VisitLiteralExpr(this);

    }

    public class Unary : Expr
    {
        public readonly Token Oper;
        public readonly Expr Right;

        public Unary(Token oper, Expr right)
        {
            this.Oper = oper;
            this.Right = right;
        }

        public override T Accept<T>(Visitor<T> visitor) => visitor.VisitUnaryExpr(this);
        public override void Accept(Visitor visitor) => visitor.VisitUnaryExpr(this);

    }

    public class Variable : Expr
    {
        public readonly Token Name;

        public Variable(Token name)
        {
            this.Name = name;
        }

        public override T? Accept<T>(Visitor<T> visitor) where T : default => visitor.VisitVariableExpr(this);
        public override void Accept(Visitor visitor)  => visitor.VisitVariableExpr(this);

    }

    public class Assign : Expr
    {
        public readonly Token Name;
        public readonly Expr Value;

        public Assign(Token name, Expr value)
        {
            this.Name = name;
            this.Value = value;
        }

        public override T? Accept<T>(Visitor<T> visitor) where T : default => visitor.VisitAssignExpr(this);
        public override void Accept(Visitor visitor)  => visitor.VisitAssignExpr(this);

    }
}

public struct UnassignedVariable
{
}

