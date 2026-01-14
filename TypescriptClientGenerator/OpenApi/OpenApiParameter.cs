namespace TypescriptClientGenerator.OpenApi;

public class OpenApiParameter
{
    public string Name { get; set; } = "";
    public string In { get; set; } = "";
    public bool Required { get; set; }
    public OpenApiSchema Schema { get; set; } = new();
}
