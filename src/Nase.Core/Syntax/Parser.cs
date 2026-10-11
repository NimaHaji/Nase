
using Nase.Core.Lexing;
using Nase.Core.Syntax.AST;

namespace Nase.Core.Syntax;

public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _position;

    public Parser(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
    }

    public CreateTableNode ParseCreateTable()
    {
        Expect(TokenKind.Create);
        Expect(TokenKind.Table);

        string firstName = Expect(TokenKind.Identifier).Lexeme;
        string? schemaName = null;
        string tableName = firstName;

        if (Match(TokenKind.Dot))
        {
            schemaName = firstName;
            tableName = Expect(TokenKind.Identifier).Lexeme;
        }

        Expect(TokenKind.LeftParenthesis);

        var columns = new List<ColumnDefinitionNode>();
        var constraints = new List<ConstraintNode>();

        while (Current.Kind != TokenKind.RightParenthesis)
        {
            if (Match(TokenKind.Constraint))
            {
                string constraintName =
                    Expect(TokenKind.Identifier).Lexeme;

                constraints.Add(
                    ParseTableConstraint(constraintName));
            }
            else if (Current.Kind is TokenKind.Primary
                     or TokenKind.Foreign)
            {
                constraints.Add(ParseTableConstraint(null));
            }
            else
            {
                columns.Add(ParseColumnDefinition());
            }

            if (!Match(TokenKind.Comma))
            {
                break;
            }
        }

        Expect(TokenKind.RightParenthesis);
        Match(TokenKind.Semicolon);

        return new CreateTableNode(
            schemaName,
            tableName,
            columns,
            constraints);
    }
    
    public SchemaNode ParseSchema()
    {
        var tables = new List<CreateTableNode>();

        while (Current.Kind != TokenKind.EndOfFile)
        {
            tables.Add(ParseCreateTable());
        }

        return new SchemaNode(tables);
    }

    private ColumnDefinitionNode ParseColumnDefinition()
    {
        string name = Expect(TokenKind.Identifier).Lexeme;
        DataTypeNode dataType = ParseDataType();

        bool isNullable = true;
        var constraints = new List<ConstraintNode>();

        while (true)
        {
            if (Match(TokenKind.Not))
            {
                Expect(TokenKind.Null);
                isNullable = false;
            }
            else if (Match(TokenKind.Null))
            {
                isNullable = true;
            }
            else if (Match(TokenKind.Unique))
            {
                constraints.Add(
                    new UniqueConstraintNode(null, [name]));
            }
            else if (Match(TokenKind.Primary))
            {
                Expect(TokenKind.Key);
                isNullable = false;

                constraints.Add(
                    new PrimaryKeyConstraintNode(null, [name]));
            }
            else
            {
                break;
            }
        }

        return new ColumnDefinitionNode(
            name,
            dataType,
            isNullable,
            constraints);
    }

    private ConstraintNode ParseTableConstraint(string? name)
    {
        if (Match(TokenKind.Primary))
        {
            Expect(TokenKind.Key);

            var columns = ParseIdentifierList();

            return new PrimaryKeyConstraintNode(name, columns);
        }

        Expect(TokenKind.Foreign);
        Expect(TokenKind.Key);

        var localColumns = ParseIdentifierList();

        Expect(TokenKind.References);

        string firstName = Expect(TokenKind.Identifier).Lexeme;
        string? referencedSchema = null;
        string referencedTable = firstName;

        if (Match(TokenKind.Dot))
        {
            referencedSchema = firstName;
            referencedTable = Expect(TokenKind.Identifier).Lexeme;
        }

        var referencedColumns = ParseIdentifierList();

        if (localColumns.Count != referencedColumns.Count)
        {
            throw Error(
                "Foreign key column count must match referenced column count.");
        }

        return new ForeignKeyConstraintNode(
            name,
            localColumns,
            referencedSchema,
            referencedTable,
            referencedColumns);
    }

    private IReadOnlyList<string> ParseIdentifierList()
    {
        Expect(TokenKind.LeftParenthesis);

        var identifiers = new List<string>
        {
            Expect(TokenKind.Identifier).Lexeme
        };

        while (Match(TokenKind.Comma))
        {
            identifiers.Add(
                Expect(TokenKind.Identifier).Lexeme);
        }

        Expect(TokenKind.RightParenthesis);

        return identifiers;
    }

    private DataTypeNode ParseDataType()
    {
        Token typeToken = Current;

        if (typeToken.Kind is not (
            TokenKind.Int or
            TokenKind.BigInt or
            TokenKind.Varchar or
            TokenKind.Nvarchar or
            TokenKind.Decimal or
            TokenKind.DateTime2))
        {
            throw Error($"Unexpected data type '{typeToken.Lexeme}'.");
        }

        Advance();

        var parameters = new List<int>();

        if (Match(TokenKind.LeftParenthesis))
        {
            parameters.Add(ReadNumber());

            while (Match(TokenKind.Comma))
            {
                parameters.Add(ReadNumber());
            }

            Expect(TokenKind.RightParenthesis);
        }

        return new DataTypeNode(typeToken.Lexeme, parameters);
    }

    private int ReadNumber()
    {
        Token token = Expect(TokenKind.Number);

        if (!int.TryParse(token.Lexeme, out int value))
        {
            throw Error($"Invalid numeric parameter '{token.Lexeme}'.");
        }

        return value;
    }

    private bool Match(TokenKind kind)
    {
        if (Current.Kind != kind)
        {
            return false;
        }

        Advance();
        return true;
    }

    private Token Expect(TokenKind kind)
    {
        if (Current.Kind != kind)
        {
            throw Error(
                $"Expected {kind}, but found {Current.Kind} " +
                $"at line {Current.Line}, column {Current.Column}.");
        }

        return Advance();
    }

    private Token Advance()
    {
        Token token = Current;

        if (token.Kind != TokenKind.EndOfFile)
        {
            _position++;
        }

        return token;
    }

    private Token Current =>
        _position < _tokens.Count
            ? _tokens[_position]
            : throw new InvalidOperationException(
                "Token stream must contain an EndOfFile token.");

    private Exception Error(string message) =>
        new InvalidOperationException(message);
}
