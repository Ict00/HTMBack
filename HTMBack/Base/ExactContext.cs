using System.Net;

namespace HTMBack.Base;

public record ExactContext(HttpListenerContext Ctx, List<UriVar> Vars);

public static  class HttpListenerContextExtensions
{
    public static ExactContext ToExact(this HttpListenerContext ctx, List<UriVar> vars)
    {
        return new(ctx, vars);
    }
}