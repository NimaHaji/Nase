namespace Nase.Core.Lexing;

public enum TokenKind
{
    Identifier,
    Number,
    String,

    Create,
    Table,
    Primary,
    Key,
    Foreign,
    References,
    Not,
    Null,
    Unique,
    Int,
    BigInt,
    Varchar,
    Nvarchar,
    Decimal,
    DateTime2,
    
    Constraint,
    
    LeftParenthesis,
    RightParenthesis,
    Comma,
    Dot,
    Semicolon,
    Star,
    Equals,

    EndOfFile,
    Unknown
}
