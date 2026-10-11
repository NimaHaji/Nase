
using Nase.Cli.Compilation;
using Nase.CodeGen.Svg;
using System.CommandLine;

namespace Nase.Cli.Commands;

public static class DiagramCommand
{
    public static Command Create()
    {
        var command = new Command(
            "diagram",
            "Compile a SQL schema into an SVG diagram.");

        var inputArgument = new Argument<FileInfo>("input")
        {
            Description = "Path to the SQL schema file."
        };

        var outputOption = new Option<FileInfo?>("-o", "--output")
        {
            Description = "Output SVG file path."
        };

        command.Arguments.Add(inputArgument);
        command.Options.Add(outputOption);

        command.SetAction(parseResult =>
        {
            var input = parseResult.GetValue(inputArgument);
            var output = parseResult.GetValue(outputOption);

            if (input is null || !input.Exists)
            {
                Console.Error.WriteLine(
                    $"Input file not found: {input?.FullName ?? "<missing>"}");

                return 2;
            }

            var outputPath = output?.FullName
                ?? Path.ChangeExtension(input.FullName, ".svg");

            try
            {
                var source = File.ReadAllText(input.FullName);
                var compiler = new SchemaCompiler();

                var result = compiler.Compile(
                    source,
                    buildIntermediateRepresentation: true);

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

                var generator = new SvgGenerator();
                var svg = generator.Generate(
                    result.IntermediateRepresentation!);

                var fullOutputPath = Path.GetFullPath(outputPath);
                var directory = Path.GetDirectoryName(fullOutputPath);

                if (directory is not null)
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(fullOutputPath, svg);

                Console.WriteLine("Diagram generated successfully.");
                Console.WriteLine(fullOutputPath);

                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(
                    $"Diagram generation failed: {exception.Message}");

                return 1;
            }
        });

        return command;
    }
}
