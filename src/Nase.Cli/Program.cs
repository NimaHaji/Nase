using Nase.Cli.Commands;
using System.CommandLine;

var rootCommand = new RootCommand(
    "Nase — Database Schema Compiler");

rootCommand.Subcommands.Add(CheckCommand.Create());
rootCommand.Subcommands.Add(DiagramCommand.Create());

return await rootCommand.Parse(args).InvokeAsync();
