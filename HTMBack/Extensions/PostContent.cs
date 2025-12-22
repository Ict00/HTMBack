using System.Net;

namespace HTMBack.Extensions;

public static class PostContent
{
    extension(HttpListenerContext ctx)
    {
        public string GetPostContent()
        {
            if (ctx.Request.HttpMethod == "POST")
            {
                using var reader = new StreamReader(ctx.Request.InputStream, ctx.Request.ContentEncoding);
                var body = reader.ReadToEnd();
                
                return body;
            }

            return "Not a POST";
        }
    }
}