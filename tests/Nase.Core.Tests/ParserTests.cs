using Nase.Core.Lexing;
using Nase.Core.Syntax;
using Nase.Core.Syntax.AST;
using Nase.FrontEnd.Lexing;

namespace Nase.Core.Tests;

public class ParserTests
{
    [Fact]
    public void ParseCreateTable_WhenValidSql_ReturnsTableWithColumns()
    {
        // Arrange
        const string sql = """
                           CREATE TABLE Users (
                               Id INT NOT NULL,
                               Name NVARCHAR(100) NULL
                           );
                           """;

        var lexer = new Lexer(sql);
        IReadOnlyList<Token> tokens = lexer.Tokenize();
        var parser = new Parser(tokens);

        // Act
        CreateTableNode table = parser.ParseCreateTable();

        // Assert
        Assert.Equal("Users", table.TableName);
        Assert.Equal(2, table.Columns.Count);

        Assert.Equal("Id", table.Columns[0].Name);
        Assert.Equal("INT", table.Columns[0].DataType.Name);
        Assert.False(table.Columns[0].IsNullable);

        Assert.Equal("Name", table.Columns[1].Name);
        Assert.Equal("NVARCHAR", table.Columns[1].DataType.Name);
        Assert.True(table.Columns[1].IsNullable);
        Assert.Equal(100, table.Columns[1].DataType.Parameters[0]);
    }
    
    
    [Fact]
    public void ParseCreateTable_WhenCompositePrimaryKeyExists_ParsesAllKeyColumns()
    {
        // Arrange
        const string sql = """
                           CREATE TABLE OrderItems (
                               UserId INT NOT NULL,
                               ProductId INT NOT NULL,
                               CONSTRAINT PK_OrderItems
                                   PRIMARY KEY (UserId, ProductId)
                           );
                           """;

        var lexer = new Lexer(sql);
        var parser = new Parser(lexer.Tokenize());

        // Act
        CreateTableNode table = parser.ParseCreateTable();

        // Assert
        var primaryKey = Assert.Single(
            table.Constraints.OfType<PrimaryKeyConstraintNode>());

        Assert.Equal("PK_OrderItems", primaryKey.Name);
        Assert.Equal(
            new[] { "UserId", "ProductId" },
            primaryKey.Columns);
    }
    
    
    [Fact]
    public void ParseCreateTable_WhenClosingParenthesisIsMissing_ThrowsException()
    {
        // Arrange
        const string sql = """
                           CREATE TABLE Users (
                               Id INT NOT NULL
                           """;

        var lexer = new Lexer(sql);
        var parser = new Parser(lexer.Tokenize());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => parser.ParseCreateTable());
    }
    
    
    [Fact]
    public void ParseCreateTable_WhenForeignKeyColumnCountsDiffer_ThrowsException()
    {
        // Arrange
        const string sql = """
                           CREATE TABLE Orders (
                               UserId INT NOT NULL,
                               CONSTRAINT FK_Orders_Users
                                   FOREIGN KEY (UserId)
                                   REFERENCES dbo.Users(Id, TenantId)
                           );
                           """;

        var lexer = new Lexer(sql);
        var parser = new Parser(lexer.Tokenize());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => parser.ParseCreateTable());
    }
    
    
    [Fact]
    public void ParseSchema_WhenMultipleTablesExist_ReturnsAllTables()
    {
        // Arrange
        const string sql = """
                           CREATE TABLE dbo.Users (
                               Id INT NOT NULL
                           );

                           CREATE TABLE dbo.Orders (
                               Id INT NOT NULL,
                               UserId INT NOT NULL
                           );
                           """;

        var lexer = new Lexer(sql);
        var parser = new Parser(lexer.Tokenize());

        // Act
        SchemaNode schema = parser.ParseSchema();

        // Assert
        Assert.Equal(2, schema.Tables.Count);

        Assert.Equal("Users", schema.Tables[0].TableName);
        Assert.Equal("dbo", schema.Tables[0].SchemaName);

        Assert.Equal("Orders", schema.Tables[1].TableName);
        Assert.Equal("dbo", schema.Tables[1].SchemaName);
    }

}
