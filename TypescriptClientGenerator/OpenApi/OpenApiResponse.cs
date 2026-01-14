namespace TypescriptClientGenerator.OpenApi;

public class OpenApiResponse
{
    public string Description { get; set; } = "";
    public Dictionary<string, OpenApiMediaType>? Content { get; set; }
}
