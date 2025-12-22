using HTMBack.Base;
using HTMBack.Components;

namespace HTMBack.EContent;

public interface IProduceContent
{
    public EContent.Content Produce(ExactContext ctx, ComponentManager componentManager);
}