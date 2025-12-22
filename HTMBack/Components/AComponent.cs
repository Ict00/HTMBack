using System.ComponentModel;
using System.Xml;
using HTMBack.Base;
using HTMBack.EContent;

namespace HTMBack.Components;

public abstract class AComponent : IProduceContent
{
    public XmlNode Node = null!;

    public abstract Content Produce(ExactContext ctx, ComponentManager componentManager);
}