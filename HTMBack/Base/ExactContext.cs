using System.Net;

namespace HTMBack.Base;

public record ExactContext(HttpListenerContext Ctx, List<UriVar> Vars, string FullUri, Dictionary<string, object> MiddleInfo);

public static  class HttpListenerContextExtensions
{
    public static ExactContext ToExact(this HttpListenerContext ctx, List<UriVar> vars, string fullUri, Dictionary<string, object> middleInfo)
    {
        return new(ctx, vars, fullUri, middleInfo);
    }
}