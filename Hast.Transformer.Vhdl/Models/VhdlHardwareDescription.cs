using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.Common;
using Hast.Common.Models;

namespace Hast.Transformer.Vhdl.Models
{
    public class VhdlHardwareDescription : IHardwareDescription
    {
        private VhdlManifest _manifest;
        private MethodIdTable _methodIdTable;

        public string Language { get { return "VHDL"; } }
        public VhdlManifest Manifest { get { return _manifest; } }
        public MethodIdTable MethodIdTable { get { return _methodIdTable; } }
        public IEnumerable<string> HardwareMembers { get { return _methodIdTable.Values.Select(mapping => mapping.MethodName); } }


        public VhdlHardwareDescription()
        {
        }

        public VhdlHardwareDescription(VhdlManifest manifest, MethodIdTable methodIdTable)
        {
            _manifest = manifest;
            _methodIdTable = methodIdTable;
        }


        public async void Save(Stream stream)
        {
            if (_manifest == null) throw new InvalidOperationException("There is no manifest to save");

            using (var writer = new StreamWriter(stream))
            {
                var storage = new Storage
                {
                    Manifest = _manifest,
                    MethodIdTable = _methodIdTable,
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
                _methodIdTable = storage.MethodIdTable;
            }
        }


        public class Storage
        {
            public VhdlManifest Manifest { get; set; }
            public MethodIdTable MethodIdTable { get; set; }
        }
    }
}
