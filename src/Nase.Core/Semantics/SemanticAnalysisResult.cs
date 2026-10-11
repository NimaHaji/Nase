namespace Nase.Core.Semantics;

public sealed record SemanticAnalysisResult(
    bool IsValid,
    IReadOnlyList<SemanticDiagnostic> Diagnostics);
