using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hast.Common.Models;
using Hast.VhdlBuilder;
using Hast.VhdlBuilder.Representation.Declaration;
using Newtonsoft.Json;

namespace Hast.Transformer.Vhdl.Models
{
    public class VhdlHardwareDescription : IHardwareDescription
    {
        private VhdlManifest _manifest;
        private MemberIdTable _memberIdTable;

        public string Language { get { return "VHDL"; } }
        public VhdlManifest Manifest { get { return _manifest; } }
        public MemberIdTable MemberIdTable { get { return _memberIdTable; } }
        public IEnumerable<string> HardwareMembers { get { return _memberIdTable.Values.Select(mapping => mapping.MemberName); } }


        public VhdlHardwareDescription()
        {
        }

        public VhdlHardwareDescription(VhdlManifest manifest, MemberIdTable memberIdTable)
        {
            _manifest = manifest;
            _memberIdTable = memberIdTable;
        }


        public int LookupMemberId(string memberFullName)
        {
            return _memberIdTable.LookupMemberId(memberFullName);
        }

        public async Task Save(Stream stream)
        {
            if (_manifest == null) throw new InvalidOperationException("There is no manifest to save.");

            using (var writer = new StreamWriter(stream))
            {
                var storage = new Storage
                {
                    Manifest = _manifest,
                    MemberIdTable = _memberIdTable,
                };



                await writer.WriteAsync(JsonConvert.SerializeObject(
                    storage, 
                    Formatting.None,
                    GetJsonSerializerSettings()));
            }
        }


        public static async Task<VhdlHardwareDescription> Load(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var storage = JsonConvert.DeserializeObject<Storage>(
                        await reader.ReadToEndAsync(),
                        GetJsonSerializerSettings());

                if (storage == null) return null;

                return new VhdlHardwareDescription(storage.Manifest, storage.MemberIdTable);
            }
        }


        private static JsonSerializerSettings GetJsonSerializerSettings()
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize
            };

            JsonSerializerSettingsPopulator.PopulateSettings(settings);

            return settings;
        }


        public class Storage
        {
            public VhdlManifest Manifest { get; set; }
            public MemberIdTable MemberIdTable { get; set; }
        }
    }
}
