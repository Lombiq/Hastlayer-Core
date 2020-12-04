using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Helpers;
using Hast.Layer;
using Hast.Transformer.Abstractions;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace Hast.Transformer
{
    public enum Language
    {
        CSharp,
        VisualBasic
    }

    public static class TransformerExtensions
    {
        public static Task<IHardwareDescription> Transform(
            this ITransformer transformer,
            string sourceCode,
            Language language,
            IHardwareGenerationConfiguration configuration)
        {
            CompilerResults result;
            var providerOptions = new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } };
            var parameters = new CompilerParameters()
            {
                GenerateInMemory = false,
                TreatWarningsAsErrors = false,
                OutputAssembly = "DynamicHastAssembly" + Sha2456Helper.ComputeHash(sourceCode)
            };

            switch (language)
            {
                case Language.CSharp:
                    result = new CSharpCodeProvider(providerOptions).CompileAssemblyFromSource(parameters, sourceCode);
                    break;
                case Language.VisualBasic:
                    result = new VBCodeProvider(providerOptions).CompileAssemblyFromSource(parameters, sourceCode);
                    break;
                default:
                    throw new ArgumentException("Unsupported .NET language.");
            }

            if (result.Errors.HasErrors)
            {
                var builder = new StringBuilder();
                foreach (var item in result.Errors) builder.Append(Environment.NewLine + item);
                throw new ArgumentException("The provided source code is invalid and has the following errors: " + builder.ToString());
            }

            return transformer.Transform(new[] { result.CompiledAssembly }, configuration);
        }
    }
}
