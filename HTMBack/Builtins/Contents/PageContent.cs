using System.Xml;
using HTMBack.Base;
using HTMBack.Components;
using HTMBack.EContent;
using HTMBack.Routing;

namespace HTMBack.Builtins.Contents;

public class PageContent : IServerContent
{
    private XmlNode _rootNode;
    private Content? _cachedContent;
    private bool _cache = false;
    
    public PageContent(string routePath, string pagePath, bool cache = false)
    {
        Route = new Route(routePath);
        _cache = cache;
        _cachedContent = null;
        
        var doc = new XmlDocument();
        doc.Load(pagePath);
        _rootNode = doc.DocumentElement!;
    }
    
    public Content Produce(ExactContext ctx, ComponentManager componentManager)
    {
        if (_cachedContent == null && _cache)
        {
            var htmlStr = componentManager.CompileXmlToHtml(_rootNode, ctx);
            _cachedContent = new Content(htmlStr);
            return _cachedContent;
        }
        if (_cache)
        {
            return _cachedContent!;
        }
        
        var str = componentManager.CompileXmlToHtml(_rootNode, ctx);
        return new Content("text/html", str);
    }

    public Route Route { get; }
}