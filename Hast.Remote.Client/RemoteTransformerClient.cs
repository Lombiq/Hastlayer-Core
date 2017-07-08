﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Common.Models;
using Hast.Transformer.Abstractions;
using Hast.Transformer.Abstractions.Extensions;

namespace Hast.Remote.Client
{
    public class RemoteTransformerClient : ITransformer
    {
        public Task<IHardwareDescription> Transform(IEnumerable<Assembly> assemblies, IHardwareGenerationConfiguration configuration)
        {
            assemblies.ThrowArgumentExceptionIfAnyInMemory();
            throw new NotImplementedException();
        }

        public Task<IHardwareDescription> Transform(IEnumerable<string> assemblyPaths, IHardwareGenerationConfiguration configuration) =>
            Transform(assemblyPaths.Select(path => Assembly.LoadFrom(path)), configuration);
    }
}
