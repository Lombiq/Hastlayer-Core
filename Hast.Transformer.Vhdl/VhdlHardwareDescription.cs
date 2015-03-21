﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Hast.VhdlBuilder.Representation.Declaration;
using Hast.Common;

namespace Hast.Transformer.Vhdl
{
    public class VhdlHardwareDescription : IHardwareDescription
    {
        private VhdlManifest _manifest;
        private MethodIdTable _callIdTable;

        public string Language { get { return "VHDL"; } }
        public VhdlManifest Manifest { get { return _manifest; } }
        public MethodIdTable CallIdTable { get { return _callIdTable; } }


        public VhdlHardwareDescription()
        {
        }

        public VhdlHardwareDescription(VhdlManifest manifest, MethodIdTable callIdTable)
        {
            _manifest = manifest;
            _callIdTable = callIdTable;
        }


        public async void Save(Stream stream)
        {
            if (_manifest == null) throw new InvalidOperationException("There is no manifest to save");

            using (var writer = new StreamWriter(stream))
            {
                var storage = new Storage
                {
                    Manifest = _manifest,
                    CallIdTable = _callIdTable,
                };

                await writer.WriteAsync(JsonConvert.SerializeObject(storage, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }));
            }
        }

        public async void Load(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var storage = await JsonConvert.DeserializeObjectAsync<Storage>(await reader.ReadToEndAsync(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                
                _manifest = storage.Manifest;
                _callIdTable = storage.CallIdTable;
            }
        }


        public class Storage
        {
            public VhdlManifest Manifest { get; set; }
            public MethodIdTable CallIdTable { get; set; }
        }
    }
}
