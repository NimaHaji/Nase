using Nase.Core.Semantics;
using Nase.Core.Syntax;
using Nase.FrontEnd.Lexing;
using Xunit;

namespace Nase.Core.Tests;

public class SemanticAnalyzerTests
{
    [Theory]
    [InlineData("Users", "dbo.Users")]
    [InlineData("dbo.Users", "DBO.USERS")]
    public void Analyze_WhenTableIsDuplicated_ReturnsSEM001(
        string firstTable,
        string secondTable)
    {
        var result = Analyze($"""
                              CREATE TABLE {firstTable} (
                                  Id INT NOT NULL
                              );

                              CREATE TABLE {secondTable} (
                                  Id INT NOT NULL
                              );
                              """);

        Assert.False(result.IsValid);

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("SEM001", diagnostic.Code);
    }

    [Fact]
    public void Analyze_WhenTablesHaveDifferentSchemas_ReturnsValid()
    {
        var result = Analyze("""
                             CREATE TABLE dbo.Users (
                                 Id INT NOT NULL
                             );

                             CREATE TABLE sales.Users (
                                 Id INT NOT NULL
                             );
                             """);

        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }

    private static SemanticAnalysisResult Analyze(string source)
    {
        var lexer = new Lexer(source);
        var parser = new Parser(lexer.Tokenize());
        var schema = parser.ParseSchema();

        var analyzer = new SemanticAnalyzer();

        return analyzer.Analyze(schema);
    }
    
    
    [Fact]
    public void Analyze_WhenPrimaryKeyReferencesMissingColumn_ReturnsSEM002()
    {
        var result = Analyze("""
                             CREATE TABLE dbo.Users (
                                 Id INT NOT NULL,
                                 PRIMARY KEY (UserId)
                             );
                             """);

        Assert.False(result.IsValid);

        var diagnostic = Assert.Single(result.Diagnostics);
        Assert.Equal("SEM002", diagnostic.Code);
    }

    [Fact]
    public void Analyze_WhenPrimaryKeyReferencesExistingColumn_ReturnsValid()
    {
        var result = Analyze("""
                             CREATE TABLE dbo.Users (
                                 Id INT NOT NULL,
                                 PRIMARY KEY (Id)
                             );
                             """);

        Assert.True(result.IsValid);
        Assert.Empty(result.Diagnostics);
    }
    
    
    [Fact]
    public void Analyze_WhenReferencedTableDoesNotExist_ReturnsSEM003()
    {
        var result = Analyze("""
                             CREATE TABLE dbo.Orders (
                                 Id INT NOT NULL,
                                 UserId INT NOT NULL,
                                 FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
                             );
                             """);

        Assert.False(result.IsValid);
        Assert.Contains(result.Diagnostics,
            diagnostic => diagnostic.Code == "SEM003");
    }

    [Fact]
    public void Analyze_WhenReferencedTableExists_ReturnsNoSEM003()
    {
        var result = Analyze("""
                             CREATE TABLE dbo.Orders (
                                 Id INT NOT NULL,
                                 UserId INT NOT NULL,
                                 FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
                             );

                             CREATE TABLE dbo.Users (
                                 Id INT NOT NULL
                             );
                             """);

        Assert.DoesNotContain(result.Diagnostics,
            diagnostic => diagnostic.Code == "SEM003");
    }
    
    
    [Fact]
    public void Analyze_WhenReferencedColumnDoesNotExist_ReturnsSEM004()
    {
        var result = Analyze("""
                             CREATE TABLE dbo.Users (
                                 Id INT NOT NULL
                             );

                             CREATE TABLE dbo.Orders (
                                 Id INT NOT NULL,
                                 UserId INT NOT NULL,
                                 FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId)
                             );
                             """);

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Diagnostics,
            diagnostic => diagnostic.Code == "SEM004");
    }

    [Fact]
    public void Analyze_WhenReferencedColumnExists_ReturnsNoSEM004()
    {
        var result = Analyze("""
                             CREATE TABLE dbo.Users (
                                 Id INT NOT NULL
                             );

                             CREATE TABLE dbo.Orders (
                                 Id INT NOT NULL,
                                 UserId INT NOT NULL,
                                 FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
                             );
                             """);

        Assert.DoesNotContain(
            result.Diagnostics,
            diagnostic => diagnostic.Code == "SEM004");
    }
    
    
    [Fact]
    public void Analyze_WhenReferencedColumnIsNotUnique_ReturnsSEM005()
    {
        var result = Analyze("""
                             CREATE TABLE dbo.Users (
                                 Id INT NOT NULL,
                                 Email NVARCHAR(100) NOT NULL
                             );

                             CREATE TABLE dbo.Orders (
                                 Id INT NOT NULL,
                                 UserEmail NVARCHAR(100) NOT NULL,
                                 FOREIGN KEY (UserEmail) REFERENCES dbo.Users(Email)
                             );
                             """);

        Assert.Contains(
            result.Diagnostics,
            diagnostic => diagnostic.Code == "SEM005");
    }

    [Fact]
    public void Analyze_WhenReferencedColumnIsPrimaryKey_ReturnsNoSEM005()
    {
        var result = Analyze("""
                             CREATE TABLE dbo.Users (
                                 Id INT NOT NULL PRIMARY KEY
                             );

                             CREATE TABLE dbo.Orders (
                                 Id INT NOT NULL,
                                 UserId INT NOT NULL,
                                 FOREIGN KEY (UserId) REFERENCES dbo.Users(Id)
                             );
                             """);

        Assert.DoesNotContain(
            result.Diagnostics,
            diagnostic => diagnostic.Code == "SEM005");
    }

    [Fact]
    public void Analyze_WhenReferencedColumnsMatchCompositePrimaryKey_ReturnsNoSEM005()
    {
        var result = Analyze("""
                             CREATE TABLE dbo.UserRoles (
                                 UserId INT NOT NULL,
                                 RoleId INT NOT NULL,
                                 PRIMARY KEY (UserId, RoleId)
                             );

                             CREATE TABLE dbo.Assignments (
                                 UserId INT NOT NULL,
                                 RoleId INT NOT NULL,
                                 FOREIGN KEY (UserId, RoleId)
                                     REFERENCES dbo.UserRoles(UserId, RoleId)
                             );
                             """);

        Assert.DoesNotContain(
            result.Diagnostics,
            diagnostic => diagnostic.Code == "SEM005");
    }

}   
