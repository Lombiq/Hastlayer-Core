using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Hast.Common.ContractResolvers;
using Hast.Layer;
using Hast.VhdlBuilder.Representation.Declaration;
using Newtonsoft.Json;

namespace Hast.Transformer.Vhdl.Models
{
    public class VhdlHardwareDescription : IHardwareDescription
    {
        public string Language { get { return "VHDL"; } }
        [JsonProperty]
        public string VhdlSource { get; private set; }
        [JsonProperty]
        public IReadOnlyDictionary<string, int> HardwareEntryPointNamesToMemberIdMappings { get; private set; }
        [JsonProperty]
        public IEnumerable<ITransformationWarning> Warnings { get; private set; }

        /// <summary>
        /// The VHDL manifest (syntax tree) of the implemented hardware. WARNING: this property is only filled if the
        /// manifest was freshly built, it will be <c>null</c> if the result comes from cache! (In both cases
        /// <see cref="VhdlSource"/> will contain the VHDL source code.)
        /// </summary>
        [JsonIgnore]
        public VhdlManifest VhdlManifestIfFresh { get; private set; }


        public VhdlHardwareDescription()
        {
        }

        public VhdlHardwareDescription(string vhdlSource, ITransformedVhdlManifest transformedVhdlManifest)
        {
            VhdlSource = vhdlSource;
            HardwareEntryPointNamesToMemberIdMappings = transformedVhdlManifest.MemberIdTable.Mappings;
            VhdlManifestIfFresh = transformedVhdlManifest.Manifest;
            Warnings = transformedVhdlManifest.Warnings;
        }


        public Task WriteSource(Stream stream)
        {
            ThroIfVhdlSourceEmpty();

            using (var streamWriter = new StreamWriter(stream))
            {
                // WriteAsync would throw a "The stream is currently in use by a previous operation on the stream." for
                // FileStreams, even though supposedly there's no operation on the stream.
                streamWriter.Write(VhdlSource);
            }

            return Task.CompletedTask;
        }

        public async Task Serialize(Stream stream)
        {
            ThroIfVhdlSourceEmpty();

            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(JsonConvert.SerializeObject(
                    this, 
                    Formatting.None,
                    GetJsonSerializerSettings()));
            }
        }


        private void ThroIfVhdlSourceEmpty()
        {
            if (string.IsNullOrEmpty(VhdlSource)) throw new InvalidOperationException("There is no VHDL source set.");
        }


        public static async Task<VhdlHardwareDescription> Deserialize(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<VhdlHardwareDescription>(
                        await reader.ReadToEndAsync(),
                        GetJsonSerializerSettings());
            }
        }


        private static JsonSerializerSettings GetJsonSerializerSettings() =>
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new PrivateSetterContractResolver(),
            };
    }
}
