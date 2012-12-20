using System.Reflection;

namespace HastTranspiler
{
    public interface ITranspiler
    {
        string Transpile(string assemplyPath);
        string Transpile(Assembly assembly);
    }
}
