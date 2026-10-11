using Nase.Core.IntermediateRepresentation;
using Nase.Core.Semantics;
using Nase.Core.Syntax;
using Nase.Core.Syntax.AST;
using Nase.FrontEnd.Lexing;

namespace Nase.Cli.Compilation;

public sealed class SchemaCompiler
{
    public SchemaCompilationResult Compile(
        string source,
        bool buildIntermediateRepresentation = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);

        var lexer = new Lexer(source);
        var tokens = lexer.Tokenize();

        var parser = new Parser(tokens);
        var schema = parser.ParseSchema();

        var analyzer = new SemanticAnalyzer();
        var analysisResult = analyzer.Analyze(schema);

        DatabaseIR? intermediateRepresentation = null;

        if (analysisResult.IsValid && buildIntermediateRepresentation)
        {
            var irBuilder = new IRBuilder();

            intermediateRepresentation = irBuilder.Build(
                schema,
                analysisResult);
        }

        return new SchemaCompilationResult(
            schema,
            analysisResult,
            intermediateRepresentation);
    }
}

public sealed record SchemaCompilationResult(
    SchemaNode Schema,
    SemanticAnalysisResult AnalysisResult,
    DatabaseIR? IntermediateRepresentation)
{
    public bool IsValid => AnalysisResult.IsValid;
}
