using System.Text;
using System.Xml;
using HTMBack.Base;

namespace HTMBack.Components;

public delegate object CompileAction(ExactContext ctx, XmlNode? node, ComponentManager manager);

public record AttributeSelector(string AttributeName, CompileAction Action, string? OnlyForTag = null);
public record TagSelector(string TagName, CompileAction Action);
public record VarSelector(string VarName, CompileAction Action);

public class ComponentManager
{
    private List<TagSelector> _tagSelectors;
    private List<VarSelector> _varSelectors;

    public ComponentManager(List<TagSelector>? tagSelectors = null,
        List<VarSelector>? varSelectors = null)
    {
        _varSelectors = varSelectors ?? [];
        _tagSelectors = tagSelectors ?? [];
    }

    public void AttachTag(string tag, CompileAction action)
    {
        _tagSelectors.Add(new TagSelector(tag, action));
    }

    public void UpdateVar(string var, CompileAction newAction)
    {
        foreach (var i in _varSelectors)
        {
            if (i.VarName == var)
            {
                _varSelectors.Remove(i);
                _varSelectors.Add(new(var, newAction));
                return;
            }
        }
        _varSelectors.Add(new(var, newAction));
    }

    public void AttachVar(string var, CompileAction action)
    {
        _varSelectors.Add(new VarSelector(var, action));
    }

    public ComponentManager DeriveWith(List<VarSelector> vars)
    {
        return new(_tagSelectors, [.._varSelectors, ..vars]);
    }
    
    public object? TryGetVarObject(string text, ExactContext ctx, ref int i, XmlNode? node = null)
    {
        if (i + 1 >= text.Length || text[i] != '{' || text[i + 1] != '{') return null;
        
        i += 2;
        string varDefinition = string.Empty;

        for (; i < text.Length; i++)
        {
            if (i + 1 < text.Length && text[i] == '}' && text[i + 1] == '}')
            {
                i += 1;
                varDefinition = varDefinition.Trim();
                break;
            }
            varDefinition += text[i];
        }

        bool doInverse = false;
        bool checkIfNull = false;

        if (varDefinition[0] == '!')
        {
            doInverse = true;
            varDefinition = varDefinition.Substring(1);
        }

        if (varDefinition[0] == '?')
        {
            checkIfNull = true;
            varDefinition = varDefinition.Substring(1);
        }

        List<string> fieldLevels = [varDefinition];

        if (varDefinition.Contains('.'))
        {
            fieldLevels = varDefinition.Split('.').ToList();
        }
                
        string varName = fieldLevels[0];
                
        var selector = _varSelectors.Find(x => x.VarName == varName);

        if (selector != null)
        {
            var obj = selector.Action(ctx, node, this);
            if (obj is bool b)
            {
                if (doInverse)
                {
                    return !b;
                }
            }
            else
            {
                if (fieldLevels.Count > 1)
                {
                    object? GetRecProperty(string property, object? objct, int index)
                    {
                        if (objct != null)
                        {
                            var newObj = objct.GetType().GetProperty(property)?.GetValue(objct);

                            if (index < fieldLevels.Count-1)
                            {
                                        
                                return GetRecProperty(fieldLevels[index+1], newObj, index+1);
                            }
                                    
                            return newObj;
                        }
                        return null;
                    }

                    var a = GetRecProperty(fieldLevels[1], obj, 1);

                    if (checkIfNull)
                    {
                        a = a == null;
                    }
                    
                    if (a is bool c)
                    {
                        if (doInverse)
                        {
                            return !c;
                        }
                        else
                        {
                            return c;
                        }
                    }
                    else
                    {
                        return a;
                    }
                }

                if (checkIfNull)
                {
                    obj = obj == null;
                    if (doInverse)
                    {
                        return !(bool)obj;
                    }
                }
                
                return obj;
            }
        }
        else
        {
            return varDefinition;
        }

        return null;
    }
    
    public string TryGetVar(string text, ExactContext ctx, XmlNode? node = null)
    {
        text = text.Trim();
        StringBuilder builder = new();

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '{')
            {
                builder.Append(TryGetVarObject(text, ctx, ref i, node));
            }
            else
            {
                builder.Append(text[i]);
            }
        }
        
        return builder.ToString();
    }

    private string TryGetVar(XmlNode node, ExactContext ctx)
    {
        string text = node.Value!;

        return TryGetVar(text, ctx, node);
    }
    
    // TODO: REWORK VAR REPLACEMENTS
    
    public string CompileXmlToHtml(XmlNode node, ExactContext ctx)
    {
        StringBuilder builder = new();

        if (node.NodeType == XmlNodeType.Element && node is  XmlElement element)
        {
            var tagName = element.Name;
            
            var selector = _tagSelectors.Find(x => x.TagName == tagName);
                
            if (selector != null)
            {
                builder.Append(selector.Action(ctx, element, this));
                return builder.ToString();
            }
            else
            {
                builder.Append($"<{tagName}");

                foreach (XmlAttribute attr in element.Attributes)
                {
                    CompileAction? action = null;

                    if (action == null)
                    {
                        var addValue = attr.Value;
                        if (addValue.StartsWith("@"))
                        {
                            addValue = TryGetVar(addValue, ctx);
                        }
                        
                        builder.Append($" {attr.Name}=\"{addValue}\"");
                    }
                    else
                    {
                        builder.Append(action(ctx, element, this));
                    }
                }
            }

            if (element.IsEmpty)
            {
                builder.Append("/>");
            }
            else
            {
                builder.Append($">");

                if (element.ChildNodes.Count == 0)
                {
                    builder.Append(TryGetVar(node.InnerText.Trim(), ctx));
                }
                else
                {
                    foreach (XmlNode cn in element.ChildNodes)
                    {
                        builder.Append(CompileXmlToHtml(cn, ctx));
                    }
                }
                
                builder.Append($"</{tagName}>");
            }
        }
        else if (node.NodeType == XmlNodeType.Text)
        {
            builder.Append(TryGetVar(node, ctx));
        }
        
        return builder.ToString();
    }
}