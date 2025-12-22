using System.Net;
using HTMBack.Base;

#pragma warning disable CS8601

namespace HTMBack.Extensions;

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

    extension(ExactContext ctx)
    {
        public Cookie? GetCookie(string name) => ctx.Ctx.GetCookie(name);
        public bool TryGetCookie(string name, out Cookie cookie) => ctx.Ctx.TryGetCookie(name, out cookie);
        public void SetCookie(Cookie cookie) => ctx.Ctx.SetCookie(cookie);
    }
}