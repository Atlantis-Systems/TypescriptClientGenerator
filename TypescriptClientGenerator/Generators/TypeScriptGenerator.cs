using System.Text;
using System.Text.Json;
using TypescriptClientGenerator.OpenApi;

namespace TypescriptClientGenerator.Generators;

public class TypeScriptGenerator
{
    private readonly OpenApiSpec _spec;

    public TypeScriptGenerator(OpenApiSpec spec)
    {
        _spec = spec;
    }

    public string Generate()
    {
        var sb = new StringBuilder();
        sb.AppendLine("// Auto-generated TypeScript client");
        sb.AppendLine("// Generated from OpenAPI specification");
        sb.AppendLine();

        GenerateModels(sb);
        GenerateClientOptions(sb);
        GenerateClient(sb);

        return sb.ToString();
    }

    private void GenerateModels(StringBuilder sb)
    {
        foreach (var (name, schema) in _spec.Components.Schemas)
        {
            sb.AppendLine($"export interface {name} {{");

            if (schema.Properties != null)
            {
                foreach (var (propName, propSchema) in schema.Properties)
                {
                    var isRequired = schema.Required?.Contains(propName) ?? false;
                    var tsType = GetTypeScriptType(propSchema);
                    var optional = isRequired ? "" : "?";
                    var camelName = ToCamelCase(propName);
                    sb.AppendLine($"  {camelName}{optional}: {tsType};");
                }
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }
    }

    private void GenerateClientOptions(StringBuilder sb)
    {
        sb.AppendLine("export interface ClientOptions {");
        sb.AppendLine("  baseUrl: string;");
        sb.AppendLine("  headers?: Record<string, string>;");
        sb.AppendLine("}");
        sb.AppendLine();
    }

    private void GenerateClient(StringBuilder sb)
    {
        var clientName = GetClientName();
        sb.AppendLine($"export class {clientName} {{");
        sb.AppendLine("  private baseUrl: string;");
        sb.AppendLine("  private headers: Record<string, string>;");
        sb.AppendLine();

        GenerateConstructor(sb);
        GenerateRequestMethod(sb);
        GenerateApiMethods(sb);

        sb.AppendLine("}");
    }

    private void GenerateConstructor(StringBuilder sb)
    {
        sb.AppendLine("  constructor(options: ClientOptions) {");
        sb.AppendLine("    this.baseUrl = options.baseUrl.replace(/\\/$/, '');");
        sb.AppendLine("    this.headers = options.headers ?? {};");
        sb.AppendLine("  }");
        sb.AppendLine();
    }

    private void GenerateRequestMethod(StringBuilder sb)
    {
        sb.AppendLine("  private async request<T>(path: string, options: RequestInit = {}): Promise<T> {");
        sb.AppendLine("    const response = await fetch(`${this.baseUrl}${path}`, {");
        sb.AppendLine("      ...options,");
        sb.AppendLine("      headers: {");
        sb.AppendLine("        'Content-Type': 'application/json',");
        sb.AppendLine("        ...this.headers,");
        sb.AppendLine("        ...options.headers,");
        sb.AppendLine("      },");
        sb.AppendLine("    });");
        sb.AppendLine();
        sb.AppendLine("    if (!response.ok) {");
        sb.AppendLine("      throw new Error(`HTTP ${response.status}: ${response.statusText}`);");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    if (response.status === 204) {");
        sb.AppendLine("      return undefined as T;");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine("    return response.json();");
        sb.AppendLine("  }");
        sb.AppendLine();
    }

    private void GenerateApiMethods(StringBuilder sb)
    {
        foreach (var (path, methods) in _spec.Paths)
        {
            foreach (var (method, operation) in methods)
            {
                var methodName = ToCamelCase(operation.OperationId);
                var parameters = GenerateMethodParameters(operation);
                var returnType = GetReturnType(operation);
                var pathWithParams = GeneratePath(path, operation);

                sb.AppendLine($"  async {methodName}({parameters}): Promise<{returnType}> {{");
                sb.AppendLine($"    return this.request<{returnType}>({pathWithParams}, {{");
                sb.AppendLine($"      method: '{method.ToUpperInvariant()}',");
                sb.AppendLine("    });");
                sb.AppendLine("  }");
                sb.AppendLine();
            }
        }
    }

    private string GetClientName()
    {
        var title = _spec.Info.Title
            .Replace(" ", "")
            .Replace("|", "")
            .Replace("-", "")
            .Trim();

        if (string.IsNullOrEmpty(title))
            return "ApiClient";

        return $"{title}Client";
    }

    private string GenerateMethodParameters(OpenApiOperation operation)
    {
        var parameters = operation.Parameters
            .Where(p => p.In == "path" || p.In == "query")
            .Select(p => $"{ToCamelCase(p.Name)}: {GetTypeScriptType(p.Schema)}")
            .ToList();

        return string.Join(", ", parameters);
    }

    private string GeneratePath(string path, OpenApiOperation operation)
    {
        var pathParams = operation.Parameters.Where(p => p.In == "path").ToList();

        if (pathParams.Count == 0)
        {
            return $"'{path}'";
        }

        var pathExpr = path;
        foreach (var param in pathParams)
        {
            pathExpr = pathExpr.Replace($"{{{param.Name}}}", $"${{{ToCamelCase(param.Name)}}}");
        }

        return $"`{pathExpr}`";
    }

    private string GetReturnType(OpenApiOperation operation)
    {
        var successResponse = operation.Responses
            .FirstOrDefault(r => r.Key == "200" || r.Key == "201");

        if (successResponse.Value?.Content == null)
        {
            return "void";
        }

        var jsonContent = successResponse.Value.Content
            .FirstOrDefault(c => c.Key.Contains("json"));

        if (jsonContent.Value == null)
        {
            return "void";
        }

        return GetTypeScriptType(jsonContent.Value.Schema);
    }

    private string GetTypeScriptType(OpenApiSchema schema)
    {
        var refValue = schema.RefProperty ?? schema.Ref;
        if (!string.IsNullOrEmpty(refValue))
        {
            var refName = refValue.Split('/').Last();
            return refName;
        }

        if (schema.Type.HasValue)
        {
            var typeElement = schema.Type.Value;
            string? typeString = null;

            if (typeElement.ValueKind == JsonValueKind.String)
            {
                typeString = typeElement.GetString();
            }
            else if (typeElement.ValueKind == JsonValueKind.Array)
            {
                var types = new List<string>();
                foreach (var t in typeElement.EnumerateArray())
                {
                    var typeName = t.GetString();
                    if (typeName != null && typeName != "null")
                    {
                        types.Add(MapPrimitiveType(typeName));
                    }
                }

                if (types.Count > 0)
                {
                    var tsType = string.Join(" | ", types);
                    if (typeElement.EnumerateArray().Any(t => t.GetString() == "null"))
                    {
                        return $"{tsType} | null";
                    }
                    return tsType;
                }
                return "unknown";
            }

            if (typeString == "array" && schema.Items != null)
            {
                var itemType = GetTypeScriptType(schema.Items);
                return $"{itemType}[]";
            }

            return MapPrimitiveType(typeString);
        }

        return "unknown";
    }

    private static string MapPrimitiveType(string? openApiType)
    {
        return openApiType switch
        {
            "string" => "string",
            "integer" => "number",
            "number" => "number",
            "boolean" => "boolean",
            "object" => "Record<string, unknown>",
            "array" => "unknown[]",
            _ => "unknown"
        };
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}
