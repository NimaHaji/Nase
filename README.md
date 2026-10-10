# Nase

Nase is a database schema compiler built with C# and .NET 10.

## Vision

Parse a documented subset of SQL Server DDL, validate its structure,
convert it into a language-independent intermediate representation,
and generate database schema diagrams.

## Architecture

- `Nase.Core`: tokens, syntax models, and intermediate schema models.
- `Nase.FrontEnd`: lexical analysis and, later, parsing and semantic analysis.
- `Nase.SqlServer`: future SQL Server metadata reader.
- `Nase.Diagrams`: future diagram generation.
- `Nase.Cli`: command-line entry point.

## Current status

The initial Lexer recognizes a small set of SQL Server keywords,
identifiers, numbers, strings, punctuation, and SQL comments.

This is an early scaffold, not a complete SQL Server parser.

## Run

```powershell
dotnet build
dotnet run --project src/Nase.Cli
```

## Development principles

- Keep the intermediate representation independent of input languages.
- Keep parsing separate from semantic validation.
- Expand SQL support through a documented grammar.
- Add automated tests before relying on compiler behavior.
