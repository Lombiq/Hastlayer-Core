using System.Reflection;
using System.Threading.Tasks;
using Orchard;

namespace Hast.Transformer
{
    /// <summary>
    /// Service for transforming a .NET assembly into hardware definition.
    /// </summary>
    public interface ITransformer : IDependency
    {
        Task<IHardwareDefinition> Transform(string assemplyPath);
        Task<IHardwareDefinition> Transform(Assembly assembly);
    }
}
