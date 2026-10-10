using Nase.Core.Lexing;
using Nase.Core.Syntax;
using Nase.Core.Syntax.AST;
using Nase.FrontEnd.Lexing;

var source = """
             CREATE TABLE Orders (
                 Id INT NOT NULL,
                 UserId INT NOT NULL,
                 CONSTRAINT PK_Orders PRIMARY KEY (Id),
                 CONSTRAINT FK_Orders_Users
                     FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
             );
             """;

var lexer = new Lexer(source);
var tokens = lexer.Tokenize();

foreach (var token in tokens)
{
    Console.WriteLine(
        $"{token.Kind,-15} {token.Lexeme}");
}

var parser = new Parser(tokens: tokens);
var table = parser.ParseCreateTable();

Console.WriteLine($"Schema: {table.SchemaName}");
Console.WriteLine($"Table: {table.TableName}");

foreach (var column in table.Columns)
{
    Console.WriteLine(
        $"{column.Name}: {column.DataType.Name}, " +
        $"Nullable={column.IsNullable}");
}

Console.WriteLine("Table Constraints:");

foreach (var constraint in table.Constraints)
{
    Console.WriteLine(
        $"{constraint.GetType().Name}: {constraint.Name}");

    switch (constraint)
    {
        case PrimaryKeyConstraintNode primaryKey:
            Console.WriteLine(
                $"  Columns: {string.Join(", ", primaryKey.Columns)}");
            break;

        case ForeignKeyConstraintNode foreignKey:
            Console.WriteLine(
                $"  Columns: {string.Join(", ", foreignKey.Columns)}");

            Console.WriteLine(
                $"  References: {foreignKey.ReferencedSchema}." +
                $"{foreignKey.ReferencedTable}" +
                $"({string.Join(", ", foreignKey.ReferencedColumns)})");
            break;
    }
}
