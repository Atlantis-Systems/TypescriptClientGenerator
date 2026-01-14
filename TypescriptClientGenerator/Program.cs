﻿using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using TypescriptClientGenerator.Generators;
using TypescriptClientGenerator.OpenApi;

var inputOption = new Option<FileInfo>("--input", "-i")
{
    Description = "The OpenAPI JSON file to generate a client from",
    Required = true
};

var outputOption = new Option<FileInfo>("--output", "-o")
{
    Description = "The output TypeScript file",
    DefaultValueFactory = _ => new FileInfo("generated.ts")
};

var clientNameOption = new Option<string?>("--client-name", "-c")
{
    Description = "The name of the generated client class (default: derived from OpenAPI title)"
};

var rootCommand = new RootCommand("Generates a TypeScript client from an OpenAPI specification")
{
    inputOption,
    outputOption,
    clientNameOption
};

rootCommand.Action = new GenerateAction(inputOption, outputOption, clientNameOption);

return await rootCommand.Parse(args).InvokeAsync();

class GenerateAction : AsynchronousCommandLineAction
{
    private readonly Option<FileInfo> _inputOption;
    private readonly Option<FileInfo> _outputOption;
    private readonly Option<string?> _clientNameOption;

    public GenerateAction(Option<FileInfo> inputOption, Option<FileInfo> outputOption, Option<string?> clientNameOption)
    {
        _inputOption = inputOption;
        _outputOption = outputOption;
        _clientNameOption = clientNameOption;
    }

    public override async Task<int> InvokeAsync(ParseResult parseResult, CancellationToken cancellationToken = default)
    {
        var inputFile = parseResult.GetValue(_inputOption)!;
        var outputFile = parseResult.GetValue(_outputOption)!;
        var clientName = parseResult.GetValue(_clientNameOption);

        if (!inputFile.Exists)
        {
            Console.Error.WriteLine($"Input file not found: {inputFile.FullName}");
            return 1;
        }

        var outputDir = outputFile.Directory;
        if (outputDir != null && !outputDir.Exists)
        {
            outputDir.Create();
        }

        var json = await File.ReadAllTextAsync(inputFile.FullName, cancellationToken);
        var openApi = JsonSerializer.Deserialize<OpenApiSpec>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (openApi == null)
        {
            Console.Error.WriteLine("Failed to parse OpenAPI specification");
            return 1;
        }

        var generator = new TypeScriptGenerator(openApi, clientName);
        var generatedCode = generator.Generate();

        await File.WriteAllTextAsync(outputFile.FullName, generatedCode, cancellationToken);
        Console.WriteLine($"Generated: {outputFile.FullName}");
        return 0;
    }
}

