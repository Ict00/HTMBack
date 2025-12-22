using System.Xml;
using HTMBack.Base;
using HTMBack.Components;
using HTMBack.EContent;
using HTMBack.Routing;

namespace HTMBack.Builtins.Contents;

public class Component : AComponent, IServerContent
{
    private Content? _cachedContent;
    private bool _cache;
    
    public Component(string routePath, string pathToXml, bool cache = false)
    {
        Route = new Route(routePath);
        _cachedContent = null;
        _cache = cache;
        var doc = new XmlDocument();
        doc.Load(pathToXml);
        Node = doc.DocumentElement!;
    }
    
    public override Content Produce(ExactContext ctx, ComponentManager componentManager)
    {
        if (_cache && _cachedContent == null) 
        {
            var compiled = componentManager.CompileXmlToHtml(Node, ctx);
            _cachedContent = new Content(compiled);
        }
        
        if (_cache) 
        {
            return _cachedContent!;
        }
        var text = componentManager.CompileXmlToHtml(Node, ctx);
        return new("text/html", text);
    }

    public Route Route { get; }
}