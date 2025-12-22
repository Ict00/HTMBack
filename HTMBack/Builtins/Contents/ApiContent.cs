using HTMBack.Base;
using HTMBack.Components;
using HTMBack.EContent;
using HTMBack.Routing;

namespace HTMBack.Builtins.Contents;

public delegate Content ContentProducer(ExactContext ctx);

public class ApiContent(string routePath, ContentProducer producer,  string method = "GET") : IServerContent
{
    public Content Produce(ExactContext ctx, ComponentManager _) => producer(ctx);

    public Route Route { get; } = new(routePath, method);
}