using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Common.Models
{
    /// <summary>
    /// Describes the hardware created from a transformed assembly, i.e. a circuit-level description of the implemented logic.
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
        /// Saves the full hardware description to a stream.
        /// </summary>
        /// <param name="stream">The stream to write the hardware description to.</param>
        void Save(Stream stream);

        /// <summary>
        /// Loads the previously saved hardware description from a stream.
        /// </summary>
        /// <param name="stream">The stream to load the hardware description from.</param>
        void Load(Stream stream);
    }


    public static class HardwareDescriptionExtensions
    {
        public static string WriteOut(this IHardwareDescription hardwareDescription)
        {
            using (var stream = new MemoryStream())
            {
                hardwareDescription.Save(stream);
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static async void ReadIn(this IHardwareDescription hardwareDescription, string content)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(content);
                }
            }
        }
    }
}
