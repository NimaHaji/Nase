namespace Nase.Core.Lexing;

public sealed record Token(
    TokenKind Kind,
    string Lexeme,
    int Line,
    int Column);
