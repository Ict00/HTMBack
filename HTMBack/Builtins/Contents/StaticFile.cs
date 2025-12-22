using HTMBack.Base;
using HTMBack.Components;
using HTMBack.EContent;
using HTMBack.Routing;

namespace HTMBack.Builtins.Contents;

public class StaticFile(string routePath, string filePath, bool cache = false, ContentTemplate? template = null) : IServerContent
{
    private string _cachedContent = string.Empty;
    private ContentTemplate? _template = template;

    public Content Produce(ExactContext ctx, ComponentManager componentManager)
    {
        if (_template == null)
        {
            _template = new();
        }
        
        if (cache)
        {
            if (_cachedContent == string.Empty)
            {
                try
                {
                    _cachedContent = File.ReadAllText(filePath);
                }
                catch
                {
                    return new Content("text/html", "<h1>500</h1>", 500);
                }
            }
            return _template.Use(_cachedContent);
        }
        try
        {
            return _template.Use(File.ReadAllText(filePath));
        }
        catch
        {
            return new Content("text/html", "<h1>500</h1>", 500);
        }
    }

    public Route Route { get; } = new(routePath);
}