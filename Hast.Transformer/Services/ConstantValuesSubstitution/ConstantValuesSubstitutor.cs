using System.Collections.Generic;
using System.Linq;
using Hast.Layer;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;

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
                new Dictionary<string, ConstantValuesSubstitutingAstProcessor.ConstructorReference>(),
                _astExpressionEvaluator)
                .SubstituteConstantValuesInSubTree(syntaxTree, false);

            return arraySizeHolder;
        }
    }
}
