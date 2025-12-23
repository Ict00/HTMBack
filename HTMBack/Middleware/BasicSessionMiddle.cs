using System.Net;
using HTMBack.Base;
using HTMBack.EContent;

namespace HTMBack.Middleware;

public class BasicSessionMiddle : AMiddleware
{
    private Dictionary<IPAddress, ulong> _sessions = [];
    private Dictionary<ulong, object> _sessionDependantData = [];
    private ulong _last;
    
    public override MiddlewareResult Process(ExactContext context, out Content? resultingContent)
    {
        resultingContent = null;
        
        if (_sessions.TryGetValue(context.Ctx.Request.RemoteEndPoint.Address, out var session))
        {
            context.MiddleInfo["session"] = session;
            return MiddlewareResult.Pass;
        }
        
        _sessions[context.Ctx.Request.RemoteEndPoint.Address] = _last++;
        context.MiddleInfo.Add("session", _last-1);
        return MiddlewareResult.Pass;
    }

    public T AddData<T>(ulong session, T data) where T : notnull
    {
        _sessionDependantData[session] = data;
        return data;
    }

    public T TryAddData<T>(ulong session, T data) where T : notnull
    {
        _sessionDependantData.TryAdd(session, data);
        return data;
    }

    public T? GetData<T>(ulong session)
    {
        if (_sessionDependantData.TryGetValue(session, out var x))
        {
            return (T)x;
        }
        return default;
    }
    
    public T GetDataOrDefault<T>(ulong session, T @default)
    {
        if (_sessionDependantData.TryGetValue(session, out var x))
        {
            return (T)x;
        }
        return @default;
    }
}