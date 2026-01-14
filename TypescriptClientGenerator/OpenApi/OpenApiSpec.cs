namespace TypescriptClientGenerator.OpenApi;

public class OpenApiSpec
{
    public string Openapi { get; set; } = "";
    public OpenApiInfo Info { get; set; } = new();
    public Dictionary<string, Dictionary<string, OpenApiOperation>> Paths { get; set; } = new();
    public OpenApiComponents Components { get; set; } = new();
}
