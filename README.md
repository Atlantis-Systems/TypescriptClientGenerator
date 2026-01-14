# TypeScript Client Generator

A .NET tool for generating TypeScript API clients from OpenAPI specifications.

## Installation

```bash
dotnet tool install --global TypescriptClientGenerator
```

## Usage

```bash
typescript-client-generator --input <openapi.json> --output <client.ts>
```

### Options

| Option | Alias | Description | Required |
|--------|-------|-------------|----------|
| `--input` | `-i` | The OpenAPI JSON file to generate a client from | Yes |
| `--output` | `-o` | The output TypeScript file (default: `generated.ts`) | No |

### Example

```bash
typescript-client-generator --input api-spec.json --output api-client.ts
```

## License

MIT
