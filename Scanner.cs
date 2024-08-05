using static TokenType;

public class Scanner
{
    private readonly string source;
    private readonly List<Token> tokens = new List<Token>();

    private int start = 0;
    private int current = 0;
    private int line = 1;

    private static readonly Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
    {
        { "and", AND },
        { "class", CLASS },
        { "else", ELSE },
        { "false", FALSE },
        { "for", FOR },
        { "fun", FUN },
        { "if", IF },
        { "nil", NIL },
        { "or", OR },
        { "print", PRINT },
        { "return", RETURN },
        { "break", BREAK },
        { "continue", CONTINUE },
        { "super", SUPER },
        { "this", THIS },
        { "true", TRUE },
        { "var", VAR },
        { "while", WHILE },
    };

    public Scanner(string source)
    {
        this.source = source;
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd())
        {
            start = current;
            ScanToken();
        }

        tokens.Add(new Token(EOF, "", null, line));

        return tokens;
    }

    private bool IsAtEnd()
    {
        return current >= source.Length;
    }

    private void ScanToken()
    {
        var c = Advance();

        switch (c)
        {
            case '(': AddToken(LEFT_PAREN); break;
            case ')': AddToken(RIGHT_PAREN); break;
            case '{': AddToken(LEFT_BRACE); break;
            case '}': AddToken(RIGHT_BRACE); break;
            case ',': AddToken(COMMA); break;
            case '.': AddToken(DOT); break;
            case '-': AddToken(MINUS); break;
            case '+': AddToken(PLUS); break;
            case ';': AddToken(SEMICOLON); break;
            case '?': AddToken(QUESTION); break;
            case ':': AddToken(COLON); break;
            case '*': AddToken(STAR); break;
            case '!':
                AddToken(Match('=') ? BANG_EQUAL : BANG);
                break;
            case '=':
                AddToken(Match('=') ? EQUAL_EQUAL : EQUAL);
                break;
            case '<':
                AddToken(Match('=') ? LESS_EQUAL : LESS);
                break;
            case '>':
                AddToken(Match('=') ? GREATER_EQUAL : GREATER);
                break;
            case '/':
                if (Match('/'))
                {
                    while (Peek() != '\n' && !IsAtEnd())
                    {
                        Advance();
                    }
                }
                else if (Match('*'))
                {
                    while (!(Peek() == '*' && PeekNext() == '/') && !IsAtEnd()) Advance();

                    if (!(Peek() == '*' && PeekNext() == '/'))
                    {
                        Lox.Error(line, "Expected */");
                        break;
                    }

                    Advance();
                    Advance();
                }
                else
                {
                    AddToken(SLASH);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                break;
            case '\n':
                line++;
                break;
            case '"':
                String();
                break;
            case '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9':
                Number();
                break;
            default:
                if (IsLetter(c))
                {
                    Identifier();
                    break;
                }

                Lox.Error(line, $"Unexpected character {c} at {current}.");
                break;
        }
    }

    private bool IsLetter(char c) => char.IsLetter(c) || c == '_';
    private bool IsLetterOrDigit(char c) => IsLetter(c) || char.IsDigit(c);

    private void Identifier()
    {
        while (IsLetterOrDigit(Peek())) Advance();

        var text = source.Substring(start, current - start);

        AddToken(_keywords.GetValueOrDefault(text, IDENTIFIER));
    }

    private void Number()
    {
        while (char.IsDigit(Peek()))
        {
            Advance();
        }

        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            Advance();

            while (char.IsDigit(Peek())) Advance();
        }

        AddToken(NUMBER, Convert.ToDouble(source.Substring(start, current - start)));
    }

    private void String()
    {
        while (Peek() != '"' && !IsAtEnd())
        {
            if (Peek() == '\n') line++;

            Advance();
        }

        Advance();

        AddToken(STRING, source.Substring(start + 1, current - 1 - (start + 1)));
    }

    private char Peek()
    {
        if (IsAtEnd())
        {
            return '\0';
        }

        return source[current];
    }

    private char PeekNext()
    {
        if (current + 1 >= source.Length)
        {
            return '\0';
        }

        return source[current + 1];
    }

    private bool Match(char expected)
    {
        if (IsAtEnd())
        {
            return false;
        }

        if (source[current] != expected)
        {
            return false;
        }

        current++;

        return true;
    }

    private char Advance()
    {
        return source[current++];
    }

    private void AddToken(TokenType type)
    {
        AddToken(type, null);
    }

    private void AddToken(TokenType type, object? literal)
    {
        var text = source.Substring(start, current - start);
        tokens.Add(new Token(type, text, literal, line));
    }

}
