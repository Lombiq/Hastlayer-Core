using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hast.Common.Models;
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


        public VhdlHardwareDescription()
        {
        }

        public VhdlHardwareDescription(string vhdlSource, MemberIdTable memberIdTable)
        {
            VhdlSource = vhdlSource;
            MemberIdTable = memberIdTable;
        }


        public int LookupMemberId(string memberFullName)
        {
            return MemberIdTable.LookupMemberId(memberFullName);
        }

        public async Task Save(Stream stream)
        {
            if (VhdlSource == null) throw new InvalidOperationException("There is no manifest to save.");

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
