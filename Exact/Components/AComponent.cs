using System.ComponentModel;
using System.Xml;
using Exact.Base;
using Exact.EContent;

namespace Exact.Components;

public abstract class AComponent : IProduceContent
{
    public XmlNode Node = null!;

    public abstract Content Produce(ExactContext ctx, ComponentManager componentManager);
}