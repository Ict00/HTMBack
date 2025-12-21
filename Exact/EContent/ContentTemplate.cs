namespace Exact.EContent;

public record ContentTemplate(string Type = "text/html", int Response = 200);

public static class ContentExtensions
{
    public static EContent.Content Use(this ContentTemplate template, string content)
    {
        return new EContent.Content(template.Type, content, template.Response);
    }
}