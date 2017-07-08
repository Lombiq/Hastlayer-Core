using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Hast.Common.Models
{
    /// <summary>
    /// Describes the hardware created from a transformed assembly, i.e. a circuit-level description of the implemented 
    /// logic.
    /// </summary>
    public interface IHardwareDescription
    {
        /// <summary>
        /// The hardware description language used.
        /// </summary>
        string Language { get; }

        /// <summary>
        /// Gets a collection of the full name of those members (including the full namespace of the parent type(s) as 
        /// well as their return type and the types of their - type - arguments) that are accessible as hardware 
        /// implementation.
        /// </summary>
        IEnumerable<string> HardwareMembers { get; }

        /// <summary>
        /// Looks up the numerical ID of the given method so the call can be identified in the hardware implementation.
        /// </summary>
        /// <param name="methodFullName">
        /// The full name (including the full namespace of the parent type(s) as well as their return type and the types 
        /// of their - type - arguments) of the method to look up the ID for.</param>
        /// <returns>The numerical ID of the method that identifies the call target in the hardware implementation.</returns>
        int LookupMemberId(string methodFullName);

        /// <summary>
        /// Writes out the hardware description's source code to the given <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> to write the source code to.</param>
        Task WriteSource(Stream stream);
    }


    public static class HardwareDescriptionExtensions
    {
        /// <summary>
        /// Writes out the hardware description's source code to a file under the given path.
        /// </summary>
        /// <param name="filePath">The full path where the file should be written to.</param>
        public static Task WriteSource(this IHardwareDescription hardwareDescription, string filePath)
        {
            using (var fileStream = File.Create(filePath))
            {
                return hardwareDescription.WriteSource(fileStream);
            }
        }
    }
}
