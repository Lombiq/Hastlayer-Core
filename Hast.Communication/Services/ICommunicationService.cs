using Hast.Transformer.SimpleMemory;
using Orchard;
using System.Threading.Tasks;

namespace Hast.Communication.Services
{
    /// <summary>
    /// Interface for implementing the basic communication with the FPGA board.
    /// </summary>
    public interface ICommunicationService : IDependency
    {
        /// <summary>
        /// The method to run the code on FPGA board.
        /// </summary>
        /// <param name="simpleMemory">The SimpleMemory object (array of bytes).</param>
        /// <param name="methodId">The method id identifies the code what we want to run on the FPGA board.</param>
        /// <returns>A SimpleMemory object.</returns>
        Task Execute(SimpleMemory simpleMemory, int methodId);
    }
}
