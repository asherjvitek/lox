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
}

