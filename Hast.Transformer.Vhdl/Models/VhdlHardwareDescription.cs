using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hast.Common.Models;
using Hast.VhdlBuilder.Representation.Declaration;
using JsonNet.PrivateSettersContractResolvers;
using Newtonsoft.Json;

namespace Hast.Transformer.Vhdl.Models
{
    public class VhdlHardwareDescription : IHardwareDescription
    {
        public string Language { get { return "VHDL"; } }
        [JsonProperty]
        public string VhdlSource { get; private set; }
        [JsonProperty]
        public MemberIdTable MemberIdTable { get; private set; }
        public IEnumerable<string> HardwareMembers { get { return MemberIdTable.Values.Select(mapping => mapping.MemberName); } }

        /// <summary>
        /// The VHDL manifest (syntax tree) of the implemented hardware. WARNING: this property is only filled if the
        /// manifest was freshly built, it will be <c>null</c> if the result comes from cache! (In both cases
        /// <see cref="VhdlSource"/> will contain the VHDL source code.)
        /// </summary>
        public VhdlManifest VhdlManifestIfFresh { get; private set; }


        public VhdlHardwareDescription()
        {
        }

        public VhdlHardwareDescription(string vhdlSource, ITransformedVhdlManifest transformedVhdlManifest)
        {
            VhdlSource = vhdlSource;
            MemberIdTable = transformedVhdlManifest.MemberIdTable;
            VhdlManifestIfFresh = transformedVhdlManifest.Manifest;
        }


        public int LookupMemberId(string memberFullName)
        {
            return MemberIdTable.LookupMemberId(memberFullName);
        }

        public async Task Save(Stream stream)
        {
            if (VhdlSource == null) throw new InvalidOperationException("There is no VHDL source to save.");

            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteAsync(JsonConvert.SerializeObject(
                    this, 
                    Formatting.None,
                    GetJsonSerializerSettings()));
            }
        }


        public static async Task<VhdlHardwareDescription> Load(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return JsonConvert.DeserializeObject<VhdlHardwareDescription>(
                        await reader.ReadToEndAsync(),
                        GetJsonSerializerSettings());
            }
        }


        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new PrivateSetterContractResolver(),
            };

            settings.Converters.Add(new MemberIdTable.MemberIdTableJsonConverter());

            return settings;
        }
    }
}
