using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;

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


        public override void VisitPrimitiveExpression(PrimitiveExpression primitiveExpression)
        {
            base.VisitPrimitiveExpression(primitiveExpression);

            // Not bothering with the various assembly attributes.
            if (primitiveExpression.FindFirstParentOfType<ICSharpCode.NRefactory.CSharp.Attribute>() != null) return;

            var primitiveExpressionParent = primitiveExpression.Parent;


            // The unnecessary type arguments for the Is<T> calls are left in because they'll be needed after
            // migrating to C# 7 and using out variables.
            ParenthesizedExpression parenthesizedExpression;
            if (primitiveExpressionParent.Is<ParenthesizedExpression>(out parenthesizedExpression) &&
                parenthesizedExpression.Expression == primitiveExpression)
            {
                var newExpression = (PrimitiveExpression)primitiveExpression.Clone();
                parenthesizedExpression.ReplaceWith(newExpression);
                primitiveExpression = newExpression;
                primitiveExpressionParent = newExpression.Parent;
            }

            CastExpression castExpression;
            if (primitiveExpressionParent.Is<CastExpression>(out castExpression))
            {
                var newExpression = new PrimitiveExpression(_astExpressionEvaluator.EvaluateCastExpression(castExpression));
                newExpression.AddAnnotation(primitiveExpressionParent.GetActualTypeReference(true));

                primitiveExpressionParent.ReplaceWith(newExpression);
                primitiveExpressionParent = newExpression.Parent;
                primitiveExpression = newExpression;
            }

            AssignmentExpression assignmentExpression;
            ArrayCreateExpression arrayCreateExpression;
            BinaryOperatorExpression binaryOperatorExpression;


            if (primitiveExpressionParent.Is<AssignmentExpression>(out assignmentExpression))
            {
                // Indexed assignments with a constant index could also be handled eventually, but not really needed
                // now.
                if (!(assignmentExpression.Left is IndexerExpression))
                {
                    _constantValuesSubstitutingAstProcessor.ConstantValuesTable.MarkAsPotentiallyConstant(
                        assignmentExpression.Left,
                        primitiveExpression,
                        primitiveExpressionParent.FindFirstParentBlockStatement()); 
                }
            }
            else if (primitiveExpressionParent.Is<ArrayCreateExpression>(out arrayCreateExpression) &&
                arrayCreateExpression.Arguments.Single() == primitiveExpression)
            {
                PassLengthOfArrayHolderToParent(arrayCreateExpression, Convert.ToInt32(primitiveExpression.Value));
            }
            else if (primitiveExpressionParent.Is<BinaryOperatorExpression>(out binaryOperatorExpression))
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

                    AssignmentExpression assignment;
                    if (binaryOperatorExpression.Parent.Is<AssignmentExpression>(out assignment))
                    {
                        _constantValuesSubstitutingAstProcessor.ConstantValuesTable.MarkAsNonConstant(assignment.Left, parentBlock);
                    }
                }
            }
            else if (primitiveExpressionParent is UnaryOperatorExpression)
            {
                // This is a unary expression where the target expression was already substituted with a const value.
                // So we can also substitute the whole expression too.
                var newExpression = new PrimitiveExpression(
                    _astExpressionEvaluator.EvaluateUnaryOperatorExpression((UnaryOperatorExpression)primitiveExpressionParent));
                newExpression.AddAnnotation(primitiveExpressionParent.Annotation<TypeInformation>());

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

                MethodDeclaration constructorDeclaration;

                if (!_constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings
                    .TryGetValue(node.GetFullName(), out constructorDeclaration))
                {
                    return;
                }

                Action<AstNode> processParent = parent =>
                    _constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings[parent.GetFullName()] =
                    constructorDeclaration;

                ProcessParent(
                    node, 
                    assignment => processParent(assignment.Left), 
                    memberReference =>
                    {
                        var memberReferenceExpressionInConstructor = ConstantValueSubstitutionHelper
                            .FindMemberReferenceInConstructor(constructorDeclaration, memberReference.GetMemberFullName(), _typeDeclarationLookupTable);

                        if (memberReferenceExpressionInConstructor != null &&
                            _constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings
                                .TryGetValue(memberReferenceExpressionInConstructor.GetFullName(), out constructorDeclaration))
                        {
                            processParent(memberReference);
                        }
                    },
                    processParent, 
                    processParent, 
                    processParent,
                    returnStatement => processParent(returnStatement.FindFirstParentEntityDeclaration()));
            }
            else
            {
                var existingSize = _arraySizeHolder.GetSize(node);

                if (existingSize == null && node is MemberReferenceExpression)
                {
                    var member = ((MemberReferenceExpression)node).FindMemberDeclaration(_typeDeclarationLookupTable);
                    if (member == null) return;
                    existingSize = _arraySizeHolder.GetSize(member);
                }

                if (existingSize == null) return;

                PassLengthOfArrayHolderToParent(node, existingSize.Length);
            }
        }


        private void PassLengthOfArrayHolderToParent(AstNode arrayHolder, int arrayLength)
        {
            Action<AstNode> processParent = parent => _arraySizeHolder.SetSize(parent, arrayLength);

            ProcessParent(
                arrayHolder,
                assignmentExpression =>
                {
                    if (assignmentExpression.Left is MemberReferenceExpression)
                    {
                        _arraySizeHolder.SetSize(
                            ((MemberReferenceExpression)assignmentExpression.Left).FindMemberDeclaration(_typeDeclarationLookupTable),
                            arrayLength);
                    }
                    _arraySizeHolder.SetSize(assignmentExpression.Left, arrayLength);
                },
                processParent,
                processParent,
                processParent,
                processParent,
                returnStatement => _arraySizeHolder.SetSize(returnStatement.FindFirstParentEntityDeclaration(), arrayLength));
        }


        private void ProcessParent(
            AstNode node,
            Action<AssignmentExpression> assignmentHandler,
            Action<MemberReferenceExpression> memberReferenceHandler,
            Action<ParameterDeclaration> invocationParameterHandler,
            Action<ParameterDeclaration> objectCreationParameterHandler,
            Action<VariableInitializer> variableInitializerHandler,
            Action<ReturnStatement> returnStatementHandler)
        {
            var parent = node.Parent;

            Action<AstNode> updateHiddenlyUpdatedNodesUpdated = n => HiddenlyUpdatedNodesUpdated.Add(n.GetFullName());

            AssignmentExpression assignmentExpression;

            if (parent.Is<AssignmentExpression>(assignment => assignment.Right == node, out assignmentExpression) ||
                parent.Is<InvocationExpression>(invocation => invocation.Target == node && invocation.Parent.Is<AssignmentExpression>(out assignmentExpression)))
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
        }
    }
}
