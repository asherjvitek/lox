using static TokenType;

public class Parser
{
    private List<Token> tokens;
    private int current = 0;
    private int loopDepth = 0;

    public Parser(List<Token> tokens)
    {
        this.tokens = tokens;
    }

    public List<Stmt> Parse()
    {
        var statements = new List<Stmt>();

        while (!IsAtEnd())
        {
            statements.AddRange(Declaration());
        }

        return statements;
    }

    private List<Stmt> Declaration()
    {
        try
        {
            var stmts = new List<Stmt>();

            if (Match(VAR))
            {
                stmts.Add(VarDeclaration());

                while (Match(COMMA))
                {
                    stmts.Add(VarDeclaration());
                }

                Consume(SEMICOLON, "Expect ';' after variable declaration.");

                return stmts;
            }

            if (Match(FUN))
            {
                return [Function("function")];
            }

            if (Match(CLASS))
            {
                return [ClassDeclaration()];
            }

            return [Statement()];
        }
        catch (ParserError)
        {
            Synchronize();
            return [];
        }
    }

    private Stmt ClassDeclaration()
    {
        var name = Consume(IDENTIFIER, $"Expect class name.");

        Consume(LEFT_BRACE, "Expect { before class body.");

        var methods = new List<Stmt.Function>();

        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            methods.Add((Function("method") as Stmt.Function)!);
        }

        Consume(RIGHT_BRACE, "Expect } after class body.");

        return new Stmt.Class(name, methods);

    }

    private Stmt Function(string kind)
    {
        var identifier = Consume(IDENTIFIER, $"Expect {kind} name.");

        Consume(LEFT_PAREN, $"Expect '(' after {kind} name.");

        var parameters = Parameters();

        Consume(RIGHT_PAREN, $"Expect ')' after parameters.");

        Consume(LEFT_BRACE, $"Expect '{{' before {kind} body.");

        var block = Block();

        return new Stmt.Function(identifier, parameters, block);
    }

    private List<Token> Parameters()
    {
        var parameters = new List<Token>();

        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }

                parameters.Add(Consume(IDENTIFIER, "Expect parameter name."));
            }
            while (Match(COMMA));
        }

        return parameters;
    }

    private Stmt VarDeclaration()
    {
        var name = Consume(IDENTIFIER, "Expect variable name");

        Expr? initializer = null;

        if (Match(EQUAL))
        {
            initializer = Expression();
        }

        return new Stmt.Var(name, initializer);
    }

    private Stmt Statement()
    {
        if (Match(FOR)) return ForStatement();
        if (Match(WHILE)) return WhileStatement();
        if (Match(IF)) return IfStatement();
        if (Match(PRINT)) return PrintStatement();
        if (Match(BREAK)) return BreakStatement();
        if (Match(CONTINUE)) return ContinueStatement();
        if (Match(RETURN)) return ReturnStatement();


        if (Match(LEFT_BRACE))
        {
            return new Stmt.Block(Block());
        }

        return ExpressionStatement();
    }

    private Stmt WhileStatement()
    {
        loopDepth++;

        Consume(LEFT_PAREN, "Expect '(' after 'while'.");

        var condition = Expression();

        Consume(RIGHT_PAREN, "Expect ')' after condition.");

        var statement = Statement();

        loopDepth--;

        return new Stmt.While(condition, statement);
    }

    private Stmt IfStatement()
    {
        Consume(LEFT_PAREN, "Expect '(' after if.");

        var condition = Expression();

        Consume(RIGHT_PAREN, "Expect ')' after condition.");

        var then = Statement();
        Stmt? elseBranch = null;

        if (Match(ELSE))
        {
            elseBranch = Statement();
        }

        return new Stmt.If(condition, then, elseBranch);
    }

    private Stmt ForStatement()
    {
        loopDepth++;

        Consume(LEFT_PAREN, "Expect '(' after 'for'.");

        Stmt? initializer = null;
        if (Match(SEMICOLON))
        {

        }
        else if (Match(VAR))
        {
            initializer = VarDeclaration();
            Consume(SEMICOLON, "Expect ';' after value.");
        }
        else
        {
            initializer = ExpressionStatement();
        }

        Expr? condition = null;
        if (!Check(SEMICOLON))
        {
            condition = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after expression");

        Expr? increment = null;
        if (!Check(RIGHT_PAREN))
        {
            increment = Expression();
        }

        Consume(RIGHT_PAREN, "Expect ')' after condition.");

        var body = Statement();

        loopDepth--;

        return new Stmt.For(initializer, condition ?? new Expr.Literal(true), body, increment);
    }

    private Stmt PrintStatement()
    {
        var value = Expression();

        Consume(SEMICOLON, "Expect ';' after value.");

        return new Stmt.Print(value);
    }

    private Stmt BreakStatement()
    {
        if (loopDepth == 0)
            throw Error(Previous(), "Expect 'break' to be inside loop");

        var breakStmt = new Stmt.Break();

        Consume(SEMICOLON, "Expect ';' after 'break'.");

        return breakStmt;
    }

    private Stmt ContinueStatement()
    {
        if (loopDepth == 0)
            throw Error(Previous(), "Expect 'continue' to be inside loop");

        var continueStmt = new Stmt.Continue();

        Consume(SEMICOLON, "Expect ';' after 'continue'.");

        return continueStmt;
    }

    private Stmt ReturnStatement()
    {
        var keyword = Previous();
        Expr? value = null;

        if (!Check(SEMICOLON))
        {
            value = Expression();
        }

        Consume(SEMICOLON, "Expect ';' after return value.");

        return new Stmt.Return(keyword, value);
    }

    private List<Stmt> Block()
    {
        var statements = new List<Stmt>();

        while (!Check(RIGHT_BRACE) && !IsAtEnd())
        {
            var decls = Declaration();

            if (decls != null)
            {
                statements.AddRange(decls);
            }
        }

        Consume(RIGHT_BRACE, "Expect '}' after block.");

        return statements;

    }

    private Stmt ExpressionStatement()
    {
        var expression = Expression();

        Consume(SEMICOLON, "Expect ';' after value.");

        return new Stmt.Expression(expression);
    }

    private Expr Expression()
    {
        return Ternary();
    }

    private Expr Ternary()
    {
        var expr = Assignment();

        if (Match(QUESTION))
        {
            var then = Expression();

            Consume(COLON, "Expect ':' after expression.");

            var elseExpression = Expression();

            return new Expr.Ternary(expr, then, elseExpression);
        }

        return expr;
    }

    private Expr Assignment()
    {
        var expr = Lambda();

        if (Match(EQUAL))
        {
            var equals = Previous();
            var value = Assignment();

            if (expr is Expr.Variable v)
            {
                return new Expr.Assign(v.Name, value);
            }
            else if (expr is Expr.Get get)
            {
                return new Expr.Set(get.Object, get.Name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr Lambda()
    {
        if (Match(FUN))
        {
            Consume(LEFT_PAREN, $"Expect '(' after 'fun' name.");

            var parameters = Parameters();

            Consume(RIGHT_PAREN, $"Expect ')' after parameters.");

            Consume(LEFT_BRACE, $"Expect '{{' before 'fun' body.");

            return new Expr.Lambda(parameters, Block());
        }

        return Or();
    }

    private Expr Or()
    {
        var expr = And();

        while (Match(OR))
        {
            var or = Previous();
            var right = And();

            return new Expr.Logical(expr, or, right);
        }

        return expr;
    }

    private Expr And()
    {
        var expr = Equality();

        while (Match(AND))
        {
            var or = Previous();
            var right = Equality();

            return new Expr.Logical(expr, or, right);
        }

        return expr;
    }

    private Expr Equality()
    {
        var expr = Comparison();

        //this had OR in the Match and I do not think that was right. Did I make that mistake?
        while (Match(BANG_EQUAL, EQUAL_EQUAL))
        {
            var oper = Previous();
            var right = Comparison();
            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr Comparison()
    {
        var expr = Term();

        while (Match(GREATER, GREATER_EQUAL, LESS, LESS_EQUAL))
        {
            var oper = Previous();
            var right = Term();
            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr Term()
    {
        var expr = Factor();

        while (Match(MINUS, PLUS))
        {
            var oper = Previous();
            var right = Factor();
            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr Factor()
    {
        var expr = Unary();

        while (Match(SLASH, STAR))
        {
            var oper = Previous();
            var right = Unary();
            expr = new Expr.Binary(expr, oper, right);
        }

        return expr;
    }

    private Expr Unary()
    {
        if (Match(BANG, MINUS))
        {
            var oper = Previous();
            return new Expr.Unary(oper, Unary());
        }

        return Call();
    }

    private Expr Call()
    {
        var expr = Primary();

        while (true)
        {
            if (Match(LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }
            else if (Match(DOT))
            {
                var name = Consume(IDENTIFIER, "Expect property name after '.'.");

                expr = new Expr.Get(expr, name);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        var arguments = new List<Expr>();

        if (!Check(RIGHT_PAREN))
        {
            do
            {
                if (arguments.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }

                arguments.Add(Expression());
            }
            while (Match(COMMA));
        }

        var paren = Consume(RIGHT_PAREN, "Expect ')' after arguments.");

        return new Expr.Call(callee, paren, arguments);
    }

    private Expr Primary()
    {
        if (Match(FALSE)) return new Expr.Literal(false);
        if (Match(TRUE)) return new Expr.Literal(true);
        if (Match(NIL)) return new Expr.Literal(null);

        if (Match(NUMBER, STRING))
        {
            return new Expr.Literal(Previous().Literal);
        }

        if (Match(THIS))
        {
            return new Expr.This(Previous());
        }

        if (Match(IDENTIFIER))
        {
            return new Expr.Variable(Previous());
        }

        if (Match(LEFT_PAREN))
        {
            var expr = Expression();

            Consume(RIGHT_PAREN, "Expect ')' after expression.");

            return new Expr.Grouping(expr);
        }

        throw Error(Peek(), "Expect expression");
    }

    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                _ = Advance();
                return true;
            }
        }

        return false;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) current++;

        return Previous();
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;

        return Peek().Type == type;
    }

    private bool IsAtEnd()
    {
        return Peek().Type == EOF;
    }

    private Token Peek()
    {
        return tokens[current];
    }

    private Token Previous()
    {
        return tokens[current - 1];
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();

        throw Error(Peek(), message);
    }

    private ParserError Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParserError();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd())
        {
            if (Previous().Type == SEMICOLON) return;

            switch (Peek().Type)
            {
                case CLASS:
                case FUN:
                case VAR:
                case FOR:
                case IF:
                case WHILE:
                case PRINT:
                case RETURN:
                    return;
                default:
                    break;
            }

            Advance();
        }
    }


    public class ParserError : Exception
    {

    }
}
