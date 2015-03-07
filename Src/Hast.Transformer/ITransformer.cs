using System.Reflection;

namespace Hast.Transformer
{
    public interface ITransformer
    {
        IHardwareDefinition Transform(string assemplyPath);
        IHardwareDefinition Transform(Assembly assembly);
    }
}
