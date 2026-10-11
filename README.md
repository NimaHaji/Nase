# Nase

**Nase** is a database schema compiler built with C# and .NET 10. It parses a supported subset of SQL Server DDL, validates database schemas, and generates SVG diagrams.

Nase is currently an **MVP** focused on implementing a real compiler pipeline with a clear, extensible architecture.

[فارسی](README.fa.md) | English

## Features

- Lexical analysis and parsing of supported SQL Server DDL
- Semantic validation of database schemas
- Intermediate representation (IR)
- SVG database schema diagram generation
- Command-line interface
- Automated tests

## Architecture

- `Nase.Core` — Core models, tokens, syntax structures, and intermediate representation.
- `Nase.FrontEnd` — Lexical analysis, parsing, and semantic analysis.
- `Nase.SqlServer` — SQL Server-specific schema support.
- `Nase.Diagrams` — Database diagram models and transformations.
- `Nase.CodeGen.Svg` — SVG generation.
- `Nase.Cli` — Command-line interface and compilation workflow.

## Requirements

- .NET 10 SDK
- Windows for the current self-contained release

## Installation

### Windows — Prebuilt Release

1. Download `nase-win-x64.zip` from the [GitHub Releases](../../releases) page.
2. Extract `nase.exe` to a permanent directory, for example `C:\Tools\Nase`.
3. Add that directory to your Windows `PATH` environment variable.
4. Open a new terminal and verify the installation:

```powershell
nase --version
nase --help
```

Nase should now be available from any directory.

> The prebuilt release is self-contained and does not require a separate .NET runtime installation.

### Build from Source

If you have the .NET 10 SDK installed, you can build and run Nase from the repository. See [Getting Started](#getting-started).

## Getting Started

Clone the repository, then build and test the solution:

```powershell
dotnet restore .\Nase.sln
dotnet build .\Nase.sln
dotnet test .\Nase.sln
```

### Validate a schema

```powershell
dotnet run --project .\src\Nase.Cli -- check .\samples\basic-schema.sql
```

### Generate an SVG diagram

```powershell
dotnet run --project .\src\Nase.Cli -- diagram .\samples\basic-schema.sql -o .\output\schema.svg
```

The generated SVG can be opened in a web browser or an SVG-compatible editor.

## Project Status

Nase is an early-stage MVP. Its supported SQL syntax is intentionally limited and will expand as the compiler and its tests evolve. The current implementation does not support the entire SQL Server language.

## Development Principles

- Keep the intermediate representation independent of the input language.
- Separate parsing from semantic validation.
- Document the supported grammar.
- Prefer incremental development and clear boundaries over unnecessary complexity.
- Use automated tests to verify compiler behavior.

## License

The project license has not yet been finalized.
