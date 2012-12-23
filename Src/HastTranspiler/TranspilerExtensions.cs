using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using Microsoft.CSharp;
using Microsoft.VisualBasic;

namespace HastTranspiler
{
    public enum Language
	{
        CSharp,
        VB
	}

    public static class TranspilerExtensions
    {
        public static IHardwareDefinition Transpile(this ITranspiler transpiler, string sourceCode, Language language)
        {
            CompilerResults result;
            var providerOptions = new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } };
            var parameters = new CompilerParameters()
            {
                GenerateInMemory = false,
                TreatWarningsAsErrors = false,
                OutputAssembly = "DynamicHastAssembly" + sourceCode.GetHashCode()
            };

            switch (language)
            {
                case Language.CSharp:
                    result = new CSharpCodeProvider(providerOptions).CompileAssemblyFromSource(parameters, sourceCode);
                    break;
                case Language.VB:
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

            return transpiler.Transpile(result.CompiledAssembly);
        }
    }
}
