using System.Net;
using HTMBack.EContent;

namespace HTMBack.Middleware;

public abstract class AMiddleware
{
    public abstract MiddlewareResult Process(HttpListenerContext context, out Content? resultingContent);
}

public enum MiddlewareResult
{
    Pass,
    Fail
}