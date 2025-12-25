using System.Collections;
using System.Text;
using System.Xml;
using HTMBack.Components;

namespace HTMBack.Builtins;

public static class Tags
{
    public static CompileAction IF_TAG = (ctx, node, mgr) =>
    {
        if (node != null && node.Attributes != null && node.Attributes["condition"] != null)
        {
            StringBuilder builder = new();
            var str =  node.Attributes["condition"]!.Value;
            
            int b = 0;
            bool condition = (bool)(mgr.TryGetVarObject(str, ctx, ref b) ?? false);

            if (condition)
            {
                foreach (XmlNode i in node.ChildNodes)
                {
                    builder.Append(mgr.CompileXmlToHtml(i, ctx));
                }
            }
            
            return builder.ToString();
        }
        
        return "";
    };

    public static CompileAction FOREACH_TAG = (ctx, node, mgr) =>
    {
        if (node != null && node.Attributes != null && node.Attributes["collection"] != null)
        {
            StringBuilder builder = new();
            var str = node.Attributes["collection"]!.Value;
            int b = 0;
            object? collection = mgr.TryGetVarObject(str, ctx, ref b);

            var newMgr = mgr.DeriveWith([]);

            if (collection is IEnumerable && collection is not string)
            {
                dynamic list = collection;
                if (list.Count > 0)
                {
                    object? first = list[0];
                    
                    if ((first.GetType().IsPrimitive || first.GetType().IsEnum) || first is string)
                    {
                        string name = "object";
                        if (node.Attributes["element"] != null)
                        {
                            name = node.Attributes["element"]!.Value;
                        }
                        
                        foreach (var i in list)
                        {
                            newMgr.UpdateVar(name, (_,_,_)=>i);
                            
                            foreach (XmlNode c in node.ChildNodes)
                            {
                                builder.Append(newMgr.CompileXmlToHtml(c, ctx));
                            }
                        }
                    }
                    else
                    {
                        foreach (object i in list)
                        {
                            if (i is not string)
                                foreach (var property in i.GetType().GetProperties())
                                {
                                    var val = property.GetValue(i);

                                    newMgr.UpdateVar(property.Name, (_, _, _) => val ?? string.Empty);
                                }

                            foreach (XmlNode c in node.ChildNodes)
                            {
                                builder.Append(newMgr.CompileXmlToHtml(c, ctx));
                            }
                        }
                    }
                }
                
                return builder.ToString();
            }
        }

        return "";
    };
}