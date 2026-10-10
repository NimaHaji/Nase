using Nase.Core.Lexing;

namespace Nase.FrontEnd.Lexing;

public sealed class Lexer
{
    private readonly string _source;
    private readonly List<Token> _tokens = [];

    private int _position;
    private int _line = 1;
    private int _column = 1;

    private static readonly Dictionary<string, TokenKind> Keywords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["CREATE"] = TokenKind.Create,
            ["TABLE"] = TokenKind.Table,
            ["PRIMARY"] = TokenKind.Primary,
            ["KEY"] = TokenKind.Key,
            ["FOREIGN"] = TokenKind.Foreign,
            ["REFERENCES"] = TokenKind.References,
            ["NOT"] = TokenKind.Not,
            ["NULL"] = TokenKind.Null,
            ["UNIQUE"] = TokenKind.Unique,
            ["INT"] = TokenKind.Int,
            ["BIGINT"] = TokenKind.BigInt,
            ["VARCHAR"] = TokenKind.Varchar,
            ["NVARCHAR"] = TokenKind.Nvarchar,
            ["DECIMAL"] = TokenKind.Decimal,
            ["DATETIME2"] = TokenKind.DateTime2
        };

    public Lexer(string source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public IReadOnlyList<Token> Tokenize()
    {
        while (!IsAtEnd)
        {
            SkipWhitespaceAndComments();

            if (IsAtEnd)
                break;

            int start = _position;
            int line = _line;
            int column = _column;
            char current = Advance();

            if (char.IsLetter(current) || current == '_' || current == '@')
            {
                ReadIdentifier(start, line, column);
                continue;
            }

            if (char.IsDigit(current))
            {
                while (!IsAtEnd && char.IsDigit(Peek()))
                    Advance();

                Add(TokenKind.Number, start, line, column);
                continue;
            }

            if (current == '\'')
            {
                ReadString(start, line, column);
                continue;
            }

            TokenKind? kind = current switch
            {
                '(' => TokenKind.LeftParenthesis,
                ')' => TokenKind.RightParenthesis,
                ',' => TokenKind.Comma,
                '.' => TokenKind.Dot,
                ';' => TokenKind.Semicolon,
                '*' => TokenKind.Star,
                '=' => TokenKind.Equals,
                _ => null
            };

            Add(kind ?? TokenKind.Unknown, start, line, column);
        }

        _tokens.Add(new Token(
            TokenKind.EndOfFile,
            string.Empty,
            _line,
            _column));

        return _tokens.AsReadOnly();
    }

    private void ReadIdentifier(int start, int line, int column)
    {
        while (!IsAtEnd &&
               (char.IsLetterOrDigit(Peek()) ||
                Peek() == '_' ||
                Peek() == '$' ||
                Peek() == '@' ||
                Peek() == '#'))
        {
            Advance();
        }

        string text = _source[start.._position];

        Add(
            Keywords.TryGetValue(text, out var kind)
                ? kind
                : TokenKind.Identifier,
            start,
            line,
            column);
    }

    private void ReadString(int start, int line, int column)
    {
        while (!IsAtEnd)
        {
            if (Advance() != '\'')
                continue;

            // SQL escapes a quote inside a string as two quotes.
            if (!IsAtEnd && Peek() == '\'')
            {
                Advance();
                continue;
            }

            Add(TokenKind.String, start, line, column);
            return;
        }

        // The parser/diagnostics layer will improve error reporting later.
        Add(TokenKind.Unknown, start, line, column);
    }

    private void SkipWhitespaceAndComments()
    {
        while (!IsAtEnd)
        {
            if (char.IsWhiteSpace(Peek()))
            {
                Advance();
                continue;
            }

            if (Peek() == '-' &&
                _position + 1 < _source.Length &&
                _source[_position + 1] == '-')
            {
                while (!IsAtEnd && Peek() != '\n')
                    Advance();

                continue;
            }

            if (Peek() == '/' &&
                _position + 1 < _source.Length &&
                _source[_position + 1] == '*')
            {
                Advance();
                Advance();

                while (!IsAtEnd)
                {
                    if (Peek() == '*' &&
                        _position + 1 < _source.Length &&
                        _source[_position + 1] == '/')
                    {
                        Advance();
                        Advance();
                        break;
                    }

                    Advance();
                }

                continue;
            }

            break;
        }
    }

    private void Add(
        TokenKind kind,
        int start,
        int line,
        int column)
    {
        _tokens.Add(new Token(
            kind,
            _source[start.._position],
            line,
            column));
    }

    private char Advance()
    {
        char current = _source[_position++];

        if (current == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        return current;
    }

    private char Peek() => _source[_position];

    private bool IsAtEnd => _position >= _source.Length;
}
