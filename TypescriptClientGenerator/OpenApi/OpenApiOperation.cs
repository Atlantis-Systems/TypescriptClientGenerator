namespace TypescriptClientGenerator.OpenApi;

public class OpenApiOperation
{
    public string OperationId { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public List<OpenApiParameter> Parameters { get; set; } = new();
    public Dictionary<string, OpenApiResponse> Responses { get; set; } = new();
}
