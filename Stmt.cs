public abstract class Stmt
{
    public abstract void Accept(Visitor visitor);
    public interface Visitor
    {
        void VisitExpressionStmt(Expression stmt);
        void VisitPrintStmt(Print stmt);
        void VisitVarStmt(Var stmt);
        void VisitBlockStmt(Block stmt);
        void VisitIfStmt(If stmt);
        void VisitWhileStmt(While stmt);
        void VisitBreakStmt(Break stmt);
        void VisitContinueStmt(Continue stmt);
        void VisitReturnStmt(Return stmt);
        void VisitForStmt(For stmt);
        void VisitFunctionStmt(Function stmt);
        void VisitClassStmt(Class stmt);
    }

    public class Expression : Stmt
    {
        public readonly Expr Expr;

        public Expression(Expr expr)
        {
            Expr = expr;
        }

        public override void Accept(Visitor visitor) => visitor.VisitExpressionStmt(this);

    }

    public class Class : Stmt
    {
        public readonly Token Name;
        public readonly List<Stmt.Function> Methods;
        public readonly List<Stmt.Function> StaticMethods;

        public Class(Token name, List<Stmt.Function> methods, List<Stmt.Function> staticMethods)
        {
            Name = name;
            Methods = methods;
            StaticMethods = staticMethods;
        }

        public override void Accept(Visitor visitor) => visitor.VisitClassStmt(this);
    }

    public class Function : Stmt
    {
        public readonly Token Name;
        public readonly List<Token> Params;
        public readonly List<Stmt> Body;

        public Function(Token name, List<Token> parameters, List<Stmt> body)
        {
            Name = name;
            Params = parameters;
            Body = body;
        }

        public override void Accept(Visitor visitor) => visitor.VisitFunctionStmt(this);

    }

    public class While : Stmt
    {
        public readonly Expr Condition;
        public readonly Stmt Body;

        public While(Expr condition, Stmt body)
        {
            Condition = condition;
            Body = body;
        }

        public override void Accept(Visitor visitor) => visitor.VisitWhileStmt(this);

    }

    public class For : Stmt
    {
        public readonly Stmt? Initializer;
        public readonly Expr Condition;
        public readonly Stmt Body;
        public readonly Expr? Incrementer;

        public For(Stmt? initializer, Expr condition, Stmt body, Expr? incrementer)
        {
            Initializer = initializer;
            Condition = condition;
            Body = body;
            Incrementer = incrementer;
        }

        public override void Accept(Visitor visitor) => visitor.VisitForStmt(this);

    }

    public class If : Stmt
    {
        public readonly Expr Condition;
        public readonly Stmt ThenBranch;
        public readonly Stmt? ElseBranch;

        public If(Expr condition, Stmt thenBranch, Stmt? elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }

        public override void Accept(Visitor visitor) => visitor.VisitIfStmt(this);

    }

    public class Print : Stmt
    {
        public readonly Expr Expr;

        public Print(Expr expr)
        {
            Expr = expr;
        }

        public override void Accept(Visitor visitor) => visitor.VisitPrintStmt(this);

    }

    public class Var : Stmt
    {
        public readonly Expr? Initializer;
        public readonly Token Name;

        public Var(Token name, Expr? initializer)
        {
            Name = name;
            Initializer = initializer;
        }

        public override void Accept(Visitor visitor) => visitor.VisitVarStmt(this);

    }

    public class Block : Stmt
    {
        public readonly List<Stmt> Statements;

        public Block(List<Stmt> statements)
        {
            Statements = statements;
        }

        public override void Accept(Visitor visitor) => visitor.VisitBlockStmt(this);

    }

    public class Break : Stmt
    {
        public override void Accept(Visitor visitor) => visitor.VisitBreakStmt(this);
    }

    public class Continue : Stmt
    {
        public override void Accept(Visitor visitor) => visitor.VisitContinueStmt(this);
    }

    public class Return : Stmt
    {
        public readonly Token Keyword;
        public readonly Expr? Value;

        public Return(Token keyword, Expr? value)
        {
            Keyword = keyword;
            Value = value;
        }

        public override void Accept(Visitor visitor) => visitor.VisitReturnStmt(this);
    }
}

