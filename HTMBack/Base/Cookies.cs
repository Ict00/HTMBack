using System.Net;
#pragma warning disable CS8601

namespace HTMBack.Base;

public static class Cookies
{
    extension(HttpListenerContext ctx)
    {
        public Cookie? GetCookie(string name)
        {
            return ctx.Request.Cookies[name];
        }

        public bool TryGetCookie(string name, out Cookie cookie)
        {
            cookie = ctx.Request.Cookies[name];
            
            return cookie != null;
        }

        public void SetCookie(Cookie cookie)
        {
            ctx.Response.SetCookie(cookie);
        }
    }
}