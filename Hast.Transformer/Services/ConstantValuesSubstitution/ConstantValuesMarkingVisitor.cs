using System;
using System.Collections.Generic;
using System.Linq;
using Hast.Transformer.Helpers;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;

namespace Hast.Transformer.Services.ConstantValuesSubstitution
{
    internal class ConstantValuesMarkingVisitor : DepthFirstAstVisitor
    {
        private readonly ConstantValuesSubstitutingAstProcessor _constantValuesSubstitutingAstProcessor;
        private readonly IAstExpressionEvaluator _astExpressionEvaluator;
        private readonly IArraySizeHolder _arraySizeHolder;
        private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;

        public HashSet<string> HiddenlyUpdatedNodesUpdated { get; } = new HashSet<string>();


        public ConstantValuesMarkingVisitor(
            ConstantValuesSubstitutingAstProcessor constantValuesSubstitutingAstProcessor,
            IAstExpressionEvaluator astExpressionEvaluator)
        {
            _constantValuesSubstitutingAstProcessor = constantValuesSubstitutingAstProcessor;
            _astExpressionEvaluator = astExpressionEvaluator;
            _arraySizeHolder = constantValuesSubstitutingAstProcessor.ArraySizeHolder;
            _typeDeclarationLookupTable = constantValuesSubstitutingAstProcessor.TypeDeclarationLookupTable;
        }


        public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            base.VisitAssignmentExpression(assignmentExpression);

            if (SimpleMemoryAssignmentHelper.IsRead4BytesAssignment(assignmentExpression))
            {
                _arraySizeHolder.SetSize(assignmentExpression.Left, 4);
            }
        }

        public override void VisitPrimitiveExpression(PrimitiveExpression primitiveExpression)
        {
            base.VisitPrimitiveExpression(primitiveExpression);

            // Not bothering with the various assembly attributes.
            if (primitiveExpression.FindFirstParentOfType<ICSharpCode.NRefactory.CSharp.Attribute>() != null) return;

            var primitiveExpressionParent = primitiveExpression.Parent;


            if (primitiveExpressionParent.Is<ParenthesizedExpression>(out var parenthesizedExpression) &&
                parenthesizedExpression.Expression == primitiveExpression)
            {
                var newExpression = (PrimitiveExpression)primitiveExpression.Clone();
                parenthesizedExpression.ReplaceWith(newExpression);
                primitiveExpression = newExpression;
                primitiveExpressionParent = newExpression.Parent;
            }

            if (primitiveExpressionParent.Is<CastExpression>(out var castExpression))
            {
                var newExpression = new PrimitiveExpression(_astExpressionEvaluator.EvaluateCastExpression(castExpression));
                newExpression.AddAnnotation(primitiveExpressionParent.GetActualTypeReference(true));

                primitiveExpressionParent.ReplaceWith(newExpression);
                primitiveExpressionParent = newExpression.Parent;
                primitiveExpression = newExpression;
            }

            // Assignments shouldn't be handled here, see ConstantValuesSubstitutingVisitor.

            if (primitiveExpressionParent.Is<ArrayCreateExpression>(expression =>
                {
                    if (expression.Arguments.Count > 1)
                    {
                        ExceptionHelper.ThrowOnlySingleDimensionalArraysSupporterException(expression);
                    }

                    return true;
                }, out var arrayCreateExpression) &&
                arrayCreateExpression.Arguments.Single() == primitiveExpression)
            {
                PassLengthOfArrayHolderToParent(arrayCreateExpression, Convert.ToInt32(primitiveExpression.Value));
            }
            else if (primitiveExpressionParent.Is<BinaryOperatorExpression>(out var binaryOperatorExpression))
            {
                var left = binaryOperatorExpression.Left;
                var right = binaryOperatorExpression.Right;

                var otherExpression = left == primitiveExpression ? right : left;

                if (otherExpression is PrimitiveExpression)
                {
                    var newExpression = new PrimitiveExpression(
                        _astExpressionEvaluator.EvaluateBinaryOperatorExpression(binaryOperatorExpression));
                    var resultType = binaryOperatorExpression.GetResultTypeReference();
                    newExpression.AddAnnotation(resultType);
                    if (!(newExpression.Value is bool) && resultType.FullName == typeof(bool).FullName)
                    {
                        newExpression.Value = newExpression.Value.ToString() == 1.ToString();
                    }

                    var parentBlock = primitiveExpressionParent.FindFirstParentBlockStatement();
                    _constantValuesSubstitutingAstProcessor.ConstantValuesTable.MarkAsPotentiallyConstant(
                        binaryOperatorExpression,
                        newExpression,
                        parentBlock);
                }
            }
            else if (primitiveExpressionParent is UnaryOperatorExpression)
            {
                // This is a unary expression where the target expression was already substituted with a const value.
                // So we can also substitute the whole expression too.
                var newExpression = new PrimitiveExpression(
                    _astExpressionEvaluator.EvaluateUnaryOperatorExpression((UnaryOperatorExpression)primitiveExpressionParent))
                    .WithAnnotation(primitiveExpressionParent.GetTypeInformationOrCreateFromActualTypeReference());

                _constantValuesSubstitutingAstProcessor.ConstantValuesTable
                    .MarkAsPotentiallyConstant(primitiveExpressionParent, newExpression, primitiveExpressionParent.Parent);
            }

            // ObjectCreateExpression, ReturnStatement, InvocationExpression are handled in GlobalValueHoldersHandlingVisitor.
        }

        protected override void VisitChildren(AstNode node)
        {
            base.VisitChildren(node);

            if (!(node is IdentifierExpression) &&
                !(node is MemberReferenceExpression))
            {
                return;
            }

            if (node.GetActualTypeReference()?.IsArray == false)
            {
                // Passing on constructor mappings.

                if (!_constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings
                    .TryGetValue(node.GetFullName(), out var constructorReference))
                {
                    return;
                }

                void processParent(AstNode parent) =>
                    _constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings[parent.GetFullName()] =
                    constructorReference;

                ProcessParent(
                    node: node,
                    assignmentHandler: assignment => processParent(assignment.Left),
                    memberReferenceHandler: memberReference =>
                    {
                        var memberReferenceExpressionInConstructor = ConstantValueSubstitutionHelper
                            .FindMemberReferenceInConstructor(constructorReference.Constructor, memberReference.GetMemberFullName(), _typeDeclarationLookupTable);

                        if (memberReferenceExpressionInConstructor != null &&
                            _constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings
                                .TryGetValue(memberReferenceExpressionInConstructor.GetFullName(), out constructorReference))
                        {
                            processParent(memberReference);
                        }
                    },
                    invocationParameterHandler: processParent,
                    objectCreationParameterHandler: processParent,
                    variableInitializerHandler: processParent,
                    returnStatementHandler: returnStatement => processParent(returnStatement.FindFirstParentEntityDeclaration()),
                    namedExpressionHandler: processParent);
            }
            else
            {
                // Passing on array sizes.

                var existingSize = _arraySizeHolder.GetSize(node);

                if (existingSize == null && node is MemberReferenceExpression)
                {
                    var member = ((MemberReferenceExpression)node).FindMemberDeclaration(_typeDeclarationLookupTable);
                    if (member == null) return;
                    existingSize = _arraySizeHolder.GetSize(member);
                    if (existingSize != null) _arraySizeHolder.SetSize(node, existingSize.Length);
                }

                if (existingSize == null) return;

                PassLengthOfArrayHolderToParent(node, existingSize.Length);
            }
        }


        private void PassLengthOfArrayHolderToParent(AstNode arrayHolder, int arrayLength)
        {
            void processParent(AstNode parent) => _arraySizeHolder.SetSize(parent, arrayLength);

            ProcessParent(
                node: arrayHolder,
                assignmentHandler: assignmentExpression =>
                {
                    if (assignmentExpression.Left is MemberReferenceExpression)
                    {
                        _arraySizeHolder.SetSize(
                            ((MemberReferenceExpression)assignmentExpression.Left).FindMemberDeclaration(_typeDeclarationLookupTable),
                            arrayLength);
                    }
                    _arraySizeHolder.SetSize(assignmentExpression.Left, arrayLength);
                },
                memberReferenceHandler: parent => { }, // This would set a size for array.Length.
                invocationParameterHandler: processParent,
                objectCreationParameterHandler: processParent,
                variableInitializerHandler: processParent,
                returnStatementHandler: returnStatement => _arraySizeHolder
                    .SetSize(returnStatement.FindFirstParentEntityDeclaration(), arrayLength),
                namedExpressionHandler: processParent);
        }


        private void ProcessParent(
            AstNode node,
            Action<AssignmentExpression> assignmentHandler,
            Action<MemberReferenceExpression> memberReferenceHandler,
            Action<ParameterDeclaration> invocationParameterHandler,
            Action<ParameterDeclaration> objectCreationParameterHandler,
            Action<VariableInitializer> variableInitializerHandler,
            Action<ReturnStatement> returnStatementHandler,
            Action<NamedExpression> namedExpressionHandler)
        {
            var parent = node.Parent;

            void updateHiddenlyUpdatedNodesUpdated(AstNode n) => HiddenlyUpdatedNodesUpdated.Add(n.GetFullName());

            if (parent.Is<AssignmentExpression>(assignment => assignment.Right == node, out var assignmentExpression) ||
                parent.Is<InvocationExpression>(invocation => invocation.Target == node && invocation.Parent.Is(out assignmentExpression)))
            {
                assignmentHandler(assignmentExpression);
                updateHiddenlyUpdatedNodesUpdated(assignmentExpression.Left);
            }
            else if (parent is MemberReferenceExpression)
            {
                memberReferenceHandler((MemberReferenceExpression)parent);
                updateHiddenlyUpdatedNodesUpdated(parent);
            }
            else if (parent is InvocationExpression)
            {
                var parameter = ConstantValueSubstitutionHelper.FindMethodParameterForPassedExpression(
                    (InvocationExpression)parent,
                    (Expression)node,
                    _typeDeclarationLookupTable);

                // The parameter can be null for special invocations like Task.WhenAll().
                if (parameter == null) return;

                invocationParameterHandler(parameter);
                updateHiddenlyUpdatedNodesUpdated(parameter);
            }
            else if (parent is ObjectCreateExpression)
            {
                var parameter = ConstantValueSubstitutionHelper.FindConstructorParameterForPassedExpression(
                        (ObjectCreateExpression)parent,
                        (Expression)node,
                        _typeDeclarationLookupTable);

                objectCreationParameterHandler(parameter);
                updateHiddenlyUpdatedNodesUpdated(parameter);
            }
            else if (parent is VariableInitializer)
            {
                variableInitializerHandler((VariableInitializer)parent);
                updateHiddenlyUpdatedNodesUpdated(parent);
            }
            else if (parent is ReturnStatement)
            {
                returnStatementHandler((ReturnStatement)parent);
                updateHiddenlyUpdatedNodesUpdated(parent);
            }
            else if (parent is NamedExpression)
            {
                // NamedExpressions are used in object initializers, e.g. new MyClass { Property = true }.
                namedExpressionHandler((NamedExpression)parent);
                updateHiddenlyUpdatedNodesUpdated(parent);
            }
        }
    }
}
