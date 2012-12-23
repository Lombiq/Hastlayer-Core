using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VhdlBuilder.Representation;

namespace HastTranspiler.Vhdl
{
    public class HardwareDefinition : IHardwareDefinition
    {
        private VhdlManifest _manifest;
        private int _maxCallId;

        public string Language { get { return "VHDL"; } }
        public VhdlManifest Manifest { get { return _manifest; } }
        public int MaxCallId { get { return _maxCallId; } }


        public HardwareDefinition()
        {
        }

        public HardwareDefinition(VhdlManifest manifest, int maxCallId)
        {
            _manifest = manifest;
            _maxCallId = maxCallId;
        }


        public async void Save(Stream stream)
        {
            if (_manifest == null) throw new InvalidOperationException("There is no manifest to save");

            using (var writer = new StreamWriter(stream))
            {
                var storage = new Storage
                {
                    Manifest = _manifest,
                    MaxCallId = _maxCallId
                };

                await writer.WriteAsync(await JsonConvert.SerializeObjectAsync(storage, Formatting.None, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto }));
            }
        }

        public async void Load(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var storage = await JsonConvert.DeserializeObjectAsync<Storage>(await reader.ReadToEndAsync(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                _manifest = storage.Manifest;
                _maxCallId = storage.MaxCallId;
            }
        }


        public class Storage
        {
            public VhdlManifest Manifest { get; set; }
            public int MaxCallId { get; set; }
        }
    }
}
