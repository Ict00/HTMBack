using Exact.Base;
using Exact.Components;

namespace Exact.EContent;

public interface IProduceContent
{
    public EContent.Content Produce(ExactContext ctx, ComponentManager componentManager);
}