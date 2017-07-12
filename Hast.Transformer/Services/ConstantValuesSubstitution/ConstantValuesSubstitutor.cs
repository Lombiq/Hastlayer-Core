using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Common.Configuration;
using Hast.Layer;
using Hast.Transformer.Abstractions.Configuration;
using Hast.Transformer.Helpers;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Orchard.Validation;

namespace Hast.Transformer.Services.ConstantValuesSubstitution
{
    public class ConstantValuesSubstitutor : IConstantValuesSubstitutor
    {
        private readonly ITypeDeclarationLookupTableFactory _typeDeclarationLookupTableFactory;
        private readonly IAstExpressionEvaluator _astExpressionEvaluator;


        public ConstantValuesSubstitutor(
            ITypeDeclarationLookupTableFactory typeDeclarationLookupTableFactory,
            IAstExpressionEvaluator astExpressionEvaluator)
        {
            _typeDeclarationLookupTableFactory = typeDeclarationLookupTableFactory;
            _astExpressionEvaluator = astExpressionEvaluator;
        }


        public IArraySizeHolder SubstituteConstantValues(SyntaxTree syntaxTree, IHardwareGenerationConfiguration configuration)
        {
            var preConfiguredArrayLengths = configuration
                .TransformerConfiguration()
                .ArrayLengths
                .ToDictionary(kvp => kvp.Key, kvp => (IArraySize)new ArraySize { Length = kvp.Value });
            var arraySizeHolder = new ArraySizeHolder(preConfiguredArrayLengths);

            new ConstantValuesSubstitutingAstProcessor(
                new ConstantValuesTable(),
                _typeDeclarationLookupTableFactory.Create(syntaxTree),
                arraySizeHolder,
                new Dictionary<string, MethodDeclaration>(),
                _astExpressionEvaluator)
                .SubstituteConstantValuesInSubTree(syntaxTree, false);

            return arraySizeHolder;
        }
    }
}
