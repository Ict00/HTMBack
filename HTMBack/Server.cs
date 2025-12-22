using System.Net;
using System.Text;
using HTMBack.Base;
using HTMBack.Builtins;
using HTMBack.Builtins.Contents;
using HTMBack.Components;
using HTMBack.EContent;
using HTMBack.Middleware;

namespace HTMBack;

public class Server
{
    private ComponentManager _componentManager;
    private HttpListener _listener;
    private List<IServerContent> _contents;
    private List<AMiddleware> _middlewares;
    
    public ComponentManager GetComponentManager() => _componentManager;

    public Server(string address, ComponentManager? componentManager = null)
    {
        _middlewares = [];
        _componentManager = componentManager ?? new ComponentManager();
        _contents = [];
        _listener = new HttpListener();
        _listener.Prefixes.Add(address);
        
        _componentManager.AttachTag("hb-if", Tags.IF_TAG);
        _componentManager.AttachTag("hb-foreach", Tags.FOREACH_TAG);
    }

    public async Task Start()
    {
        _listener.Start();
        Console.WriteLine("Started > ");
        try
        {
            while (_listener.IsListening) {
                try
                {
                    var ctx = await _listener.GetContextAsync();
                    var request = ctx.Request;

                    bool doContinue = false;

                    foreach (var i in _middlewares)
                    {
                        var result = i.Process(ctx, out var x);
                        if (result == MiddlewareResult.Fail)
                        {
                            var responseContent = x ?? new Content("text/html",
                                "<h1>Request couldn't be satisfied</h1>", 403);
                            doContinue = true;

                            byte[] buf = Encoding.UTF8.GetBytes(responseContent.Text);
                            ctx.Response.ContentLength64 = buf.Length;
                            ctx.Response.StatusCode = responseContent.ResponseMsg;
                            ctx.Response.ContentType = responseContent.Type;

                            ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                        }
                    }

                    if (doContinue)
                        continue;

                    List<UriVar> vars = [];
                    string newPath = "/";

                    if (request.RawUrl != null)
                    {
                        foreach (var i in request.RawUrl.Split('/'))
                        {
                            if (i != "")
                            {
                                newPath += $"{i.Split('?')[0]}/";
                                vars.AddRange(i.GetVars());
                            }
                        }
                    }

                    newPath = newPath.Replace("//", "/");

                    Console.WriteLine($"Requested > {newPath}");

                    var returnContent = new Content("text/html", "<h1>Not found</h1>", 404);

                    foreach (var i in _contents)
                    {
                        if (i.Route.Path == newPath && i.Route.Type == ctx.Request.HttpMethod)
                        {
                            returnContent = i.Produce(ctx.ToExact(vars), _componentManager);
                            break;
                        }
                    }

                    byte[] buffer = Encoding.UTF8.GetBytes(returnContent.Text);
                    ctx.Response.ContentLength64 = buffer.Length;
                    ctx.Response.StatusCode = returnContent.ResponseMsg;
                    ctx.Response.ContentType = returnContent.Type;
                    ctx.Response.KeepAlive = returnContent.KeepAlive;

                    await ctx.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex}");
                }
            }
        }
        catch
        {
            // Ignore
        }
    }

    public void Using<T>() where T : AMiddleware, new()
    {
        _middlewares.Add(new T());
    }

    public void AddFile(string filePath, string? name = null, bool cached = false, string fileType = "text/html")
    {
        if (!FileExists(filePath))
        {
            Console.WriteLine($"File '{filePath}' not found");
            Environment.Exit(0);
        }
        
        if (name == null)
        {
            name = new FileInfo(filePath).Name;
        }
        
        var file = new StaticFile($"/htmb_api/{name}/", filePath, cached, new ContentTemplate(fileType));
        
        _contents.Add(file);
    }

    public void AddVar(string var, CompileAction action)
    {
        _componentManager.AttachVar(var, action);
    }

    public void AddApi(string apiPath, ContentProducer producer, string method = "GET")
    {
        apiPath = FormatRoutePath(apiPath);
        
        var api = new ApiContent($"/htmb_api/{apiPath}/", producer, method);
        _contents.Add(api);
    }

    private bool FileExists(string path)
    {
        return File.Exists(path);
    }
    
    private string FormatRoutePath(string routePath, bool noSlashes = true)
    {
        if (routePath.StartsWith("/"))
        {
            if (noSlashes)
                routePath = routePath.Substring(1);
        }
        else if (!noSlashes)
        {
            routePath = routePath.Insert(0, "/");
        }

        if (routePath.EndsWith("/"))
        {
            if (noSlashes)
                routePath = routePath.Substring(0, routePath.Length - 1);
        }
        else if (!noSlashes)
        {
            routePath += "/";
        }
        
        return routePath;
    }

    public void AddPage(string pagePath, string routePath = "", bool cached = false)
    {
        pagePath = $"www/pages/{pagePath}";

        if (!FileExists(pagePath))
        {
            Console.WriteLine($"Page '{pagePath}' not found");
            Environment.Exit(0);
        }

        if (routePath == "")
        {
            if (pagePath == "main")
            {
                routePath = "/";
            }
        }
        else
        {
            routePath = FormatRoutePath(routePath, false);
        }
        
        var page = new PageContent($"{routePath}", pagePath, cached);
        
        _contents.Add(page);
    }

    public void RegisterComponent(string filePath, string? name = null, bool cached = false, bool serve = false)
    {
        filePath = $"www/components/{filePath}";
        
        if (!FileExists(filePath))
        {
            Console.WriteLine($"Component '{filePath}' not found");
            Environment.Exit(0);
        }
        
        if (name == null)
        {
            name = new FileInfo(filePath).Name.Split('.')[0];
        }
        
        var component = new Component($"/htmb_cs/{name}/", filePath, cached);
        
        _componentManager.AttachTag(name, (x, y, z) => component.Produce(x, z).Text);
        if (serve)
            _contents.Add(component);
    }
}