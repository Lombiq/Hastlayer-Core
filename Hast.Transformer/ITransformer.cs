using System.Reflection;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Orchard;

namespace Hast.Transformer
{
    /// <summary>
    /// Service for transforming a .NET assembly into hardware definition.
    /// </summary>
    public interface ITransformer : IDependency
    {
        Task<IHardwareDefinition> Transform(string assemplyPath, IHardwareGenerationConfiguration configuration);
        Task<IHardwareDefinition> Transform(Assembly assembly, IHardwareGenerationConfiguration configuration);
    }
}
