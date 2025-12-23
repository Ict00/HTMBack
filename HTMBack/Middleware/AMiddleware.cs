using System.Net;
using HTMBack.Base;
using HTMBack.EContent;

namespace HTMBack.Middleware;

public abstract class AMiddleware
{
    public abstract MiddlewareResult Process(ExactContext context, out Content? resultingContent);
}

public enum MiddlewareResult
{
    Pass,
    Fail
}