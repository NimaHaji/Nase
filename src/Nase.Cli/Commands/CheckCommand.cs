using Nase.Cli.Compilation;
using System.CommandLine;

namespace Nase.Cli.Commands;

public static class CheckCommand
{
    public static Command Create()
    {
        var command = new Command(
            "check",
            "Validate a SQL schema without generating output.");

        var inputArgument = new Argument<FileInfo>("input")
        {
            Description = "Path to the SQL schema file."
        };

        command.Arguments.Add(inputArgument);

        command.SetAction(parseResult =>
        {
            var input = parseResult.GetValue(inputArgument);

            if (input is null || !input.Exists)
            {
                Console.Error.WriteLine(
                    $"Input file not found: {input?.FullName ?? "<missing>"}");

                return 2;
            }

            try
            {
                var source = File.ReadAllText(input.FullName);
                var compiler = new SchemaCompiler();
                var result = compiler.Compile(source);

                if (!result.IsValid)
                {
                    Console.Error.WriteLine(
                        "Semantic analysis failed.");

                    foreach (var diagnostic in result.AnalysisResult.Diagnostics)
                    {
                        Console.Error.WriteLine(
                            $"{diagnostic.Code}: {diagnostic.Message}");
                    }

                    return 1;
                }

                Console.WriteLine("Schema is valid.");
                Console.WriteLine(
                    $"Tables: {result.Schema.Tables.Count}");

                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    $"Compilation failed: {exception.Message}");

                return 1;
            }
        });

        return command;
    }
}
