using Nase.Core.Semantics;
using Nase.Core.Syntax;
using Nase.FrontEnd.Lexing;

var source = """
             CREATE TABLE Users (
                 Id INT NOT NULL
             );

             CREATE TABLE dbo.Users (
                 Id INT NOT NULL
             );
             """;

var lexer = new Lexer(source);
var tokens = lexer.Tokenize();

var parser = new Parser(tokens);
var schema = parser.ParseSchema();

Console.WriteLine($"Parsed tables: {schema.Tables.Count}");

var analyzer = new SemanticAnalyzer();
var result = analyzer.Analyze(schema);

if (result.IsValid)
{
    Console.WriteLine("Semantic Analysis: Passed");
}
else
{
    Console.WriteLine("Semantic Analysis: Failed");

    foreach (var diagnostic in result.Diagnostics)
    {
        Console.WriteLine(
            $"{diagnostic.Code}: {diagnostic.Message}");
    }
}
