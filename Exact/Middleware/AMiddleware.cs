using System.Net;
using Exact.EContent;

namespace Exact.Middleware;

public abstract class AMiddleware
{
    public abstract MiddlewareResult Process(HttpListenerContext context, out Content? resultingContent);
}

public enum MiddlewareResult
{
    Pass,
    Fail
}