using Nase.Core.IntermediateRepresentation;
using Nase.Core.Semantics;
using Nase.Core.Syntax;
using Nase.Core.Syntax.AST;
using Nase.FrontEnd.Lexing;

namespace Nase.Core.Tests;

public class IRBuilderTests
{
    [Fact]
    public void Build_WhenSchemaIsValid_CreatesTablesKeysAndForeignKeys()
    {
        var source = """
                     CREATE TABLE Users (
                         Id INT NOT NULL PRIMARY KEY,
                         Email NVARCHAR(100) NOT NULL UNIQUE
                     );

                     CREATE TABLE Orders (
                         Id INT NOT NULL PRIMARY KEY,
                         UserId INT NOT NULL,
                         FOREIGN KEY (UserId) REFERENCES Users(Id)
                     );
                     """;

        var lexer = new Lexer(source);
        var parser = new Parser(lexer.Tokenize());
        var schema = parser.ParseSchema();

        var analyzer = new SemanticAnalyzer();
        var analysisResult = analyzer.Analyze(schema);

        Assert.True(analysisResult.IsValid);

        var builder = new IRBuilder();
        var ir = builder.Build(schema, analysisResult);

        Assert.Equal(2, ir.Tables.Count);
        Assert.Single(ir.ForeignKeys);

        var users = Assert.Single(
            ir.Tables.Where(table => table.Name == "Users"));

        Assert.Equal("dbo", users.SchemaName);
        Assert.Equal(2, users.Columns.Count);
        Assert.Contains(
            users.Keys,
            key => key.Kind == KeyKind.Primary &&
                   key.Columns.SequenceEqual(["Id"]));

        Assert.Contains(
            users.Keys,
            key => key.Kind == KeyKind.Unique &&
                   key.Columns.SequenceEqual(["Email"]));

        var foreignKey = Assert.Single(ir.ForeignKeys);

        Assert.Equal("dbo.Orders", foreignKey.SourceTable);
        Assert.Equal("dbo.Users", foreignKey.TargetTable);
        Assert.Equal(["UserId"], foreignKey.SourceColumns);
        Assert.Equal(["Id"], foreignKey.TargetColumns);
    }
}
