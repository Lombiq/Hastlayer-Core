using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hast.Transformer
{
    public interface IHardwareDefinition
    {
        string Language { get; }
        void Save(Stream stream);
        void Load(Stream stream);
    }

    public static class HardwareDefinitionExtensions
    {
        public static string WriteOut(this IHardwareDefinition hardwareDefinition)
        {
            using (var stream = new MemoryStream())
            {
                hardwareDefinition.Save(stream);
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        public static async void ReadIn(this IHardwareDefinition hardwareDefinition, string content)
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
