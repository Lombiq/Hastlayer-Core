using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer
{
    /// <summary>
    /// Represents the hardware created from a transformed assembly.
    /// </summary>
    public interface IHardwareDescription
    {
        /// <summary>
        /// The hardware description language used.
        /// </summary>
        string Language { get; }

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
