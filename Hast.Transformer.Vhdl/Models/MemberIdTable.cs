﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hast.Transformer.Vhdl.Models
{
    public class MemberMapping
    {
        public string MemberName { get; set; }
        public int Id { get; set; }
    }


    /// <summary>
    /// Maps class members to generated IDs that the hardware-implemented logic uses. A member acces in .NET is thus 
    /// transferred as a call to a member ID and this member will determine which part of the logic will execute.
    /// </summary>
    public class MemberIdTable
    {
        private static MemberIdTable _emptyInstance;
        private readonly Dictionary<string, MemberMapping> _mappings = new Dictionary<string, MemberMapping>();

        public static MemberIdTable Empty
        {
            get
            {
                if (_emptyInstance == null) _emptyInstance = new MemberIdTable();
                return _emptyInstance;
            }
        }

        public int MaxId { get; private set; }

        [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
        public IEnumerable<MemberMapping> Values { get { return _mappings.Values; } }


        public void SetMapping(string memberFullName, int id)
        {
            if (id > MaxId) MaxId = id;
            if (!_mappings.ContainsKey(memberFullName))
            {
                _mappings[memberFullName] = new MemberMapping { MemberName = memberFullName, Id = id };
            }
            else _mappings[memberFullName].Id = id;
        }

        public int LookupMemberId(string memberFullName)
        {
            MemberMapping mapping;
            if (_mappings.TryGetValue(memberFullName, out mapping)) return mapping.Id;
            throw new InvalidOperationException("No member ID mapping found for the given member name: " + memberFullName);
        }


        public class MemberIdTableJsonConverter : JsonConverter
        {
            public override bool CanWrite { get { return false; } }


            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(MemberIdTable);
            }

            public override object ReadJson(
                JsonReader reader,
                Type objectType,
                object existingValue,
                JsonSerializer serializer)
            {
                var jObject = JObject.Load(reader);

                var table = new MemberIdTable();
                var dummyMapping = new MemberMapping();

                foreach (var mappingJObject in jObject[nameof(table.Values)])
                {
                    table.SetMapping((string)mappingJObject[nameof(dummyMapping.MemberName)], (int)mappingJObject[nameof(dummyMapping.Id)]);
                }

                return table;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
            }
        }
    }
}
