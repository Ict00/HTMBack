using System.Net;
using System.Text;
using Exact.Base;
using Exact.Builtins.Contents;
using Exact.Components;
using Exact.EContent;
using Exact.Middleware;

namespace Exact;

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
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("Started > ");
        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                while (_listener.IsListening)
                {
                    var ctx = _listener.GetContext();
                    var request = ctx.Request;
                    
                    bool doContinue = false;
                    
                    foreach (var i in _middlewares)
                    {
                        var result = i.Process(ctx, out var x);
                        if (result == MiddlewareResult.Fail)
                        {
                            var responseContent = x ?? new Content("text/html", "<h1>Request couldn't be satisfied</h1>", 403);
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
                    
                    ctx.Response.OutputStream.Write(buffer, 0, buffer.Length);
                }
            }
            catch
            {
                //
            }
        });
    }

    public void Using<T>() where T : AMiddleware, new()
    {
        _middlewares.Add(new T());
    }

    public void AddFile(string filePath, string? name = null, bool cached = false)
    {
        if (name == null)
        {
            name = new FileInfo(filePath).Name.Split('.')[0];
        }
        
        var file = new StaticFile($"/exact_api/{name}/", filePath, cached);
        
        _contents.Add(file);
    }

    public void AddVar(string var, CompileAction action)
    {
        _componentManager.AttachVar(var, action);
    }

    public void AddApi(string apiPath, ContentProducer producer, string method = "GET")
    {
        var api = new ApiContent($"/exact_api/{apiPath}/", producer, method);
        _contents.Add(api);
    }

    public void AddPage(string pagePath, string routePath = "", bool cached = false)
    {
        pagePath = $"www/pages/{pagePath}";
        
        if (routePath == "")
        {
            if (pagePath == "main")
            {
                routePath = "/";
            }
        }
        
        var page = new PageContent($"{routePath}", pagePath, cached);
        
        _contents.Add(page);
    }

    public void RegisterComponent(string filePath, string? name = null, bool cached = false, bool serve = false)
    {
        filePath = $"www/components/{filePath}";
        if (name == null)
        {
            name = new FileInfo(filePath).Name.Split('.')[0];
        }
        
        var component = new Component($"/exact_cs/{name}/", filePath, cached);
        
        _componentManager.AttachTag(name, (x, y, z) => component.Produce(x, z).Text);
        if (serve)
            _contents.Add(component);
    }
}