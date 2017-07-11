﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace System.Collections.Generic
{
    public static class DictionaryExtensions
    {
        public static T GetOrAddCustomConfiguration<T>(this IDictionary<string, object> customConfiguration, string key)
            where T : new()
        {
            object config;

            if (customConfiguration.TryGetValue(key, out config))
            {
                // If this is a remote transformation then custom configs won't necessarily be properly deserialized at
                // this point, so need to handle them specifically.
                if (config is JObject)
                {
                    return ((JObject)config).ToObject<T>();
                }

                return (T)config;
            }

            return (T)(customConfiguration[key] = new T());
        }
    }
}
