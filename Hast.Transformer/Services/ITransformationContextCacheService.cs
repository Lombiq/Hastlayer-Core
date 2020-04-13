﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using Hast.Common.Interfaces;

namespace Hast.Transformer.Services
{
    public interface ITransformationContextCacheService : IDependency
    {
        ITransformationContext GetTransformationContext(IEnumerable<string> assemblyPaths, string transformationId);
        void SetTransformationContext(ITransformationContext transformationContext, IEnumerable<string> assemblyPaths);
    }
}
