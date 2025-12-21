using System.Text;
using System.Xml;
using Exact.Base;

namespace Exact.Components;

public delegate object CompileAction(ExactContext ctx, XmlNode? node, ComponentManager manager);

public record AttributeSelector(string AttributeName, CompileAction Action, string? OnlyForTag = null);
public record TagSelector(string TagName, CompileAction Action);
public record VarSelector(string VarName, CompileAction Action);

public class ComponentManager
{
    private Dictionary<(string? tag, string attr), CompileAction> _attributeSelectors;
    private List<TagSelector> _tagSelectors;
    private List<VarSelector> _varSelectors;

    public ComponentManager(List<TagSelector>? tagSelectors = null,
        List<AttributeSelector>? attributeSelectors = null,
        List<VarSelector>? varSelectors = null)
    {
        _varSelectors = varSelectors ?? [];
        _tagSelectors = tagSelectors ?? [];
        _attributeSelectors = new Dictionary<(string? tag, string attr), CompileAction>();
        
        if (attributeSelectors != null)
        {
            foreach (var i in attributeSelectors)
            {
                var tag = i.OnlyForTag;
                var attr = i.AttributeName;
                _attributeSelectors.Add((tag, attr), i.Action);
            }
        }
    }

    public void AttachTag(string tag, CompileAction action)
    {
        _tagSelectors.Add(new TagSelector(tag, action));
    }

    public void AttachAttribute(string attr, CompileAction action, string? onlyForTag = null)
    {
        _attributeSelectors.Add((onlyForTag, attr), action);
    }

    public void AttachVar(string var, CompileAction action)
    {
        _varSelectors.Add(new VarSelector(var, action));
    }

    public ComponentManager DeriveWith(List<VarSelector> vars)
    {
        return new([.._tagSelectors], [], [.._varSelectors, ..vars]);
    }
    
    private string TryGetVar(string text, ExactContext ctx)
    {
        text = text.Trim();
        if (text.StartsWith('@'))
        {
            var varName = text.Substring(1);
            var selector = _varSelectors.Find(x => x.VarName == varName);

            if (selector != null)
            {
                return selector.Action(ctx, null, this)?.ToString() ?? "";
            }
        }

        return text;
    }

    private string TryGetVar(XmlNode node, ExactContext ctx)
    {
        string text = node.Value!;
        text = text.Trim();
        if (text.StartsWith('@'))
        {
            var varName = text.Substring(1);
            var selector = _varSelectors.Find(x => x.VarName == varName);

            if (selector != null)
            {
                return selector.Action(ctx, node, this)?.ToString() ?? "";
            }
        }

        return text;
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
            }
            else
            {
                builder.Append($"<{tagName}");

                foreach (XmlAttribute attr in element.Attributes)
                {
                    CompileAction? action = null;
                    if (_attributeSelectors.ContainsKey((tagName, attr.Name)))
                    {
                        action = _attributeSelectors[(tagName, attr.Name)];
                    }
                    
                    
                    if (action == null)
                    {
                        if (_attributeSelectors.ContainsKey((null, attr.Value)))
                        {
                            action = _attributeSelectors[(null, attr.Name)];
                        }
                    }

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
                builder.Remove(builder.Length - 1, 1);
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