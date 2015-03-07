using System.Reflection;

namespace Hast.Transformer
{
    public interface ITranspiler
    {
        IHardwareDefinition Transpile(string assemplyPath);
        IHardwareDefinition Transpile(Assembly assembly);
    }
}
