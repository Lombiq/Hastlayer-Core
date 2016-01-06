using Hast.Transformer.SimpleMemory;
using Orchard;
using System.Threading.Tasks;
using Hast.Communication.Models;

namespace Hast.Communication.Services
{
    /// <summary>
    /// Interface for implementing the basic communication with the FPGA board.
    /// </summary>
    public interface ICommunicationService : IDependency
    {
        /// <summary>
        /// Executes the given member on hardware.
        /// </summary>
        /// <param name="simpleMemory">The <see cref="SimpleMemory"/> object representing the memory space the logic works in.</param>
        /// <param name="memberId">The member ID identifies the class member that we want to run on the FPGA board.</param>
        /// <returns>
        /// An <see cref="IExecutionInformation"/> object containing debug and runtime information about the hardware execution.
        /// </returns>
        Task<IExecutionInformation> Execute(SimpleMemory simpleMemory, int memberId);
    }
}
