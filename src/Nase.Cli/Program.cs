using Nase.Core.Lexing;
using Nase.FrontEnd.Lexing;

const string source = """
CREATE TABLE dbo.Users (
    Id INT NOT NULL,
    Name NVARCHAR(100) NULL
);
""";

var lexer = new Lexer(source);
var tokens = lexer.Tokenize();

foreach (Token token in tokens)
{
    Console.WriteLine(
        $"{token.Line}:{token.Column} " +
        $"{token.Kind,-18} {token.Lexeme}");

    if (token.Kind == TokenKind.Unknown)
    {
        Console.Error.WriteLine(
            $"Unknown token at {token.Line}:{token.Column}.");
    }
}
