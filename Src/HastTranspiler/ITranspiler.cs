using System.Reflection;

namespace HastTranspiler
{
    public interface ITranspiler
    {
        IHardwareDefinition Transpile(string assemplyPath);
        IHardwareDefinition Transpile(Assembly assembly);
    }
}
