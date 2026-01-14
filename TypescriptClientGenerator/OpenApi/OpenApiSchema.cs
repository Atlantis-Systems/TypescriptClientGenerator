using System.Text.Json;
using System.Text.Json.Serialization;

namespace TypescriptClientGenerator.OpenApi;

public class OpenApiSchema
{
    public JsonElement? Type { get; set; }
    public string? Format { get; set; }
    public string? Ref { get; set; }
    public OpenApiSchema? Items { get; set; }
    public Dictionary<string, OpenApiSchema>? Properties { get; set; }
    public List<string>? Required { get; set; }

    [JsonPropertyName("$ref")]
    public string? RefProperty { get; set; }
}
