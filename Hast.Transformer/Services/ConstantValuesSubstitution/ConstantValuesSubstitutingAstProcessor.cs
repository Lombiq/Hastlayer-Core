using System;
using System.Collections.Generic;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Services.ConstantValuesSubstitution
{
    internal class ConstantValuesSubstitutingAstProcessor
    {
        private ConstantValuesTable _constantValuesTable;
        public ConstantValuesTable ConstantValuesTable { get { return _constantValuesTable; } }

        private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;
        public ITypeDeclarationLookupTable TypeDeclarationLookupTable { get { return _typeDeclarationLookupTable; } }

        private readonly IArraySizeHolder _arraySizeHolder;
        public IArraySizeHolder ArraySizeHolder { get { return _arraySizeHolder; } }

        private readonly Dictionary<string, MethodDeclaration> _objectHoldersToConstructorsMappings;
        public Dictionary<string, MethodDeclaration> ObjectHoldersToConstructorsMappings { get { return _objectHoldersToConstructorsMappings; } }

        private readonly IAstExpressionEvaluator _astExpressionEvaluator;
        public IAstExpressionEvaluator AstExpressionEvaluator { get { return _astExpressionEvaluator; } }


        public ConstantValuesSubstitutingAstProcessor(
            ConstantValuesTable constantValuesTable,
            ITypeDeclarationLookupTable typeDeclarationLookupTable,
            IArraySizeHolder arraySizeHolder,
            Dictionary<string, MethodDeclaration> objectHoldersToConstructorsMappings,
            IAstExpressionEvaluator astExpressionEvaluator)
        {
            _constantValuesTable = constantValuesTable;
            _typeDeclarationLookupTable = typeDeclarationLookupTable;
            _arraySizeHolder = arraySizeHolder;
            _objectHoldersToConstructorsMappings = objectHoldersToConstructorsMappings;
            _astExpressionEvaluator = astExpressionEvaluator;
        }


        public void SubstituteConstantValuesInSubTree(AstNode rootNode, bool reUseOriginalConstantValuesTable)
        {
            // Gradually propagating the constant values through the syntax tree so this needs multiple passes. So 
            // running them until nothing changes.

            ConstantValuesTable originalConstantValuesTable = null;
            if (reUseOriginalConstantValuesTable) originalConstantValuesTable = _constantValuesTable.Clone();

            var constantValuesMarkingVisitor = new ConstantValuesMarkingVisitor(this, _astExpressionEvaluator);
            var objectHoldersToSubstitutedConstructorsMappingVisitor =
                new ObjectHoldersToSubstitutedConstructorsMappingVisitor(this);
            var globalValueHoldersHandlingVisitor = new GlobalValueHoldersHandlingVisitor(this, rootNode);
            var constantValuesSubstitutingVisitor = new ConstantValuesSubstitutingVisitor(this);

            string codeOutput;
            var hiddenlyUpdatedNodesUpdatedCount = 0;
            var passCount = 0;
            const int maxPassCount = 1000;
            do
            {
                codeOutput = rootNode.ToString();
                // Uncomment the below line to get a debug output about the current state of this sub-tree.
                //System.IO.File.WriteAllText("ConstantSubstitution.cs", codeOutput);
                hiddenlyUpdatedNodesUpdatedCount = constantValuesMarkingVisitor.HiddenlyUpdatedNodesUpdated.Count;

                rootNode.AcceptVisitor(constantValuesMarkingVisitor);
                rootNode.AcceptVisitor(objectHoldersToSubstitutedConstructorsMappingVisitor);
                rootNode.AcceptVisitor(globalValueHoldersHandlingVisitor);
                rootNode.AcceptVisitor(constantValuesSubstitutingVisitor);

                if (reUseOriginalConstantValuesTable) _constantValuesTable.OverWrite(originalConstantValuesTable.Clone());
                else _constantValuesTable.Clear();

                passCount++;
            } while ((codeOutput != rootNode.ToString() ||
                        constantValuesMarkingVisitor.HiddenlyUpdatedNodesUpdated.Count != hiddenlyUpdatedNodesUpdatedCount) &&
                    passCount < maxPassCount);

            if (passCount == maxPassCount)
            {
                throw new InvalidOperationException(
                    "Constant substitution needs more than " + maxPassCount +
                    "passes through the syntax tree starting with the root node " + rootNode.GetFullName() +
                    ". This most possibly indicates some error or the assembly being processed is exceptionally big.");
            }
        }
    }
}
