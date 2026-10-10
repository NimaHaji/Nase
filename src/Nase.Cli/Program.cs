using Nase.Core.Lexing;
using Nase.FrontEnd.Lexing;

var source = """
             CREATE TABLE [Order Details] (
                 Id INT NOT NULL
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
