using Exact.Base;
using Exact.Components;
using Exact.EContent;
using Exact.Routing;

namespace Exact.Builtins.Contents;

public delegate Content ContentProducer(ExactContext ctx);

public class ApiContent(string routePath, ContentProducer producer,  string method = "GET") : IServerContent
{
    public Content Produce(ExactContext ctx, ComponentManager _) => producer(ctx);

    public Route Route { get; } = new(routePath, method);
}