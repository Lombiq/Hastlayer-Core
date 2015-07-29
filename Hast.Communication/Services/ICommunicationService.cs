using Hast.Transformer.SimpleMemory;
using Orchard;
using System.Threading.Tasks;

namespace Hast.Communication.Services
{
    /// <summary>
    /// To use the fpga board with orchard this interface is what we must implement.
    /// </summary>
    public interface ICommunicationService : IDependency
    {
        /// <summary>
        /// The method to run the code on fpga board.
        /// </summary>
        /// <param name="input">The SimpleMemory object (array of bytes)</param>
        /// <param name="methodId">The method id is needed when we want to run multiple methods on fpga board.</param>
        /// <returns>A SimpleMemory object.</returns>
        Task<SimpleMemory> Execute(SimpleMemory input, int methodId);
    }
}
