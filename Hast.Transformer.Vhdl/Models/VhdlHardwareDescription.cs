using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hast.Common.Models;
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

        public async void Save(Stream stream)
        {
            if (_manifest == null) throw new InvalidOperationException("There is no manifest to save");

            using (var writer = new StreamWriter(stream))
            {
                var storage = new Storage
                {
                    Manifest = _manifest,
                    MemberIdTable = _memberIdTable,
                };

                await writer.WriteAsync(JsonConvert.SerializeObject(storage, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }));
            }
        }

        public async void Load(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var storage = JsonConvert.DeserializeObject<Storage>(await reader.ReadToEndAsync(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                
                _manifest = storage.Manifest;
                _memberIdTable = storage.MemberIdTable;
            }
        }


        public class Storage
        {
            public VhdlManifest Manifest { get; set; }
            public MemberIdTable MemberIdTable { get; set; }
        }
    }
}
