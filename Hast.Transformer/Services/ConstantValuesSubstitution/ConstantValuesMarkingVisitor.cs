using Hast.Transformer.Helpers;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.CSharp.Syntax;
using ICSharpCode.Decompiler.TypeSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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

            if (SimpleMemoryAssignmentHelper.IsBatchedReadAssignment(assignmentExpression, out var cellCount))
            {
                _arraySizeHolder.SetSize(assignmentExpression.Left, cellCount);
            }
        }

        public override void VisitPrimitiveExpression(PrimitiveExpression primitiveExpression)
        {
            base.VisitPrimitiveExpression(primitiveExpression);

            // Not bothering with the various assembly attributes.
            if (primitiveExpression.FindFirstParentOfType<ICSharpCode.Decompiler.CSharp.Syntax.Attribute>() != null) return;

            var primitiveExpressionParent = primitiveExpression.Parent;

            if (primitiveExpressionParent.Is<ParenthesizedExpression>(out var parenthesizedExpression) &&
                parenthesizedExpression.Expression == primitiveExpression)
            {
                var newExpression = primitiveExpression.Clone<PrimitiveExpression>();
                parenthesizedExpression.ReplaceWith(newExpression);
                primitiveExpression = newExpression;
                primitiveExpressionParent = newExpression.Parent;
            }

            if (primitiveExpressionParent.Is<CastExpression>(out var castExpression))
            {
                var newExpression = new PrimitiveExpression(_astExpressionEvaluator.EvaluateCastExpression(castExpression));
                newExpression.AddAnnotation(primitiveExpressionParent.CreateResolveResultFromActualType());

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
                PassLengthOfArrayHolderToParent(arrayCreateExpression, Convert.ToInt32(primitiveExpression.Value, CultureInfo.InvariantCulture));
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
                    var resultType = binaryOperatorExpression.GetResultType();
                    newExpression.AddAnnotation(resultType.ToResolveResult());
                    if (!(newExpression.Value is bool) && resultType.GetFullName() == typeof(bool).FullName)
                    {
                        newExpression.Value = newExpression.Value.ToString() == "1";
                    }

                    var parentBlock = primitiveExpressionParent.FindFirstParentBlockStatement();
                    _constantValuesSubstitutingAstProcessor.ConstantValuesTable.MarkAsPotentiallyConstant(
                        binaryOperatorExpression,
                        newExpression,
                        parentBlock);
                }
            }
            else if (primitiveExpressionParent is UnaryOperatorExpression unaryOperatorExpression)
            {
                // This is a unary expression where the target expression was already substituted with a const value.
                // So we can also substitute the whole expression.
                var newExpression = new PrimitiveExpression(
                    _astExpressionEvaluator.EvaluateUnaryOperatorExpression(unaryOperatorExpression))
                    .WithAnnotation(primitiveExpressionParent.CreateResolveResultFromActualType());

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

            if (node.GetActualType()?.IsArray() == false)
            {
                // Passing on constructor mappings.

                if (!_constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings
                    .TryGetValue(node.GetFullName(), out var constructorReference))
                {
                    return;
                }

                void ProcessParent(AstNode parent) =>
                    _constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings[parent.GetFullName()] =
                    constructorReference;

                this.ProcessParent(
                    node: node,
                    assignmentHandler: assignment => ProcessParent(assignment.Left),
                    memberReferenceHandler: memberReference =>
                    {
                        var memberReferenceExpressionInConstructor = ConstantValueSubstitutionHelper
                            .FindMemberReferenceInConstructor(constructorReference.Constructor, memberReference.GetMemberFullName(), _typeDeclarationLookupTable);

                        if (memberReferenceExpressionInConstructor != null &&
                            _constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings
                                .TryGetValue(memberReferenceExpressionInConstructor.GetFullName(), out constructorReference))
                        {
                            ProcessParent(memberReference);
                        }
                    },
                    invocationParameterHandler: ProcessParent,
                    objectCreationParameterHandler: ProcessParent,
                    variableInitializerHandler: ProcessParent,
                    returnStatementHandler: returnStatement => ProcessParent(returnStatement.FindFirstParentEntityDeclaration()),
                    namedExpressionHandler: ProcessParent);
            }
            else
            {
                // Passing on array sizes.

                var existingSize = _arraySizeHolder.GetSize(node);

                if (existingSize == null && node is MemberReferenceExpression memberReferenceExpression)
                {
                    var member = memberReferenceExpression.FindMemberDeclaration(_typeDeclarationLookupTable);
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
            void ProcessParent(AstNode parent) => _arraySizeHolder.SetSize(parent, arrayLength);

            this.ProcessParent(
                node: arrayHolder,
                assignmentHandler: assignmentExpression =>
                {
                    if (assignmentExpression.Left is MemberReferenceExpression memberReferenceExpression)
                    {
                        _arraySizeHolder.SetSize(
                            memberReferenceExpression.FindMemberDeclaration(_typeDeclarationLookupTable),
                            arrayLength);
                    }

                    _arraySizeHolder.SetSize(assignmentExpression.Left, arrayLength);
                },
                memberReferenceHandler: parent => { }, // This would set a size for array.Length.
                invocationParameterHandler: ProcessParent,
                objectCreationParameterHandler: ProcessParent,
                variableInitializerHandler: ProcessParent,
                returnStatementHandler: returnStatement => _arraySizeHolder
                    .SetSize(returnStatement.FindFirstParentEntityDeclaration(), arrayLength),
                namedExpressionHandler: ProcessParent);
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

            void UpdateHiddenlyUpdatedNodesUpdated(AstNode n) => HiddenlyUpdatedNodesUpdated.Add(n.GetFullName());

            if (parent.Is<AssignmentExpression>(assignment => assignment.Right == node, out var assignmentExpression) ||
                parent.Is<InvocationExpression>(invocation => invocation.Target == node && invocation.Parent.Is(out assignmentExpression)))
            {
                assignmentHandler(assignmentExpression);
                UpdateHiddenlyUpdatedNodesUpdated(assignmentExpression.Left);
            }
            else if (parent is MemberReferenceExpression memberReferenceExpression)
            {
                memberReferenceHandler(memberReferenceExpression);
                UpdateHiddenlyUpdatedNodesUpdated(parent);
            }
            else if (parent is InvocationExpression expression)
            {
                var parameter = ConstantValueSubstitutionHelper.FindMethodParameterForPassedExpression(
                    expression,
                    (Expression)node,
                    _typeDeclarationLookupTable);

                // There will be no parameter if the affected node is the invoked member itself. Also, the parameter 
                // can be null for special invocations like Task.WhenAll().
                if (parameter == null)
                {
                    ProcessParent(
                        node: node.Parent,
                        assignmentHandler: assignmentHandler,
                        memberReferenceHandler: memberReferenceHandler,
                        invocationParameterHandler: invocationParameterHandler,
                        objectCreationParameterHandler: objectCreationParameterHandler,
                        variableInitializerHandler: variableInitializerHandler,
                        returnStatementHandler: returnStatementHandler,
                        namedExpressionHandler: namedExpressionHandler);

                    return;
                }

                invocationParameterHandler(parameter);
                UpdateHiddenlyUpdatedNodesUpdated(parameter);
            }
            else if (parent is ObjectCreateExpression objectCreateExpression)
            {
                var parameter = ConstantValueSubstitutionHelper.FindConstructorParameterForPassedExpression(
                        objectCreateExpression,
                        (Expression)node,
                        _typeDeclarationLookupTable);

                // The parameter will be null for a Task body's delegate invocation, e.g.:
                // new Func<object, int[]> (<>c__DisplayClass6_.<ParallelizedCalculateIntegerSumUpToNumber>b__0)
                if (parameter == null)
                {
                    return;
                }

                objectCreationParameterHandler(parameter);
                UpdateHiddenlyUpdatedNodesUpdated(parameter);
            }
            else if (parent is VariableInitializer initializer)
            {
                variableInitializerHandler(initializer);
                UpdateHiddenlyUpdatedNodesUpdated(parent);
            }
            else if (parent is ReturnStatement statement)
            {
                returnStatementHandler(statement);
                UpdateHiddenlyUpdatedNodesUpdated(parent);
            }
            else if (parent is NamedExpression namedExpression)
            {
                // NamedExpressions are used in object initializers, e.g. new MyClass { Property = true }.
                namedExpressionHandler(namedExpression);
                UpdateHiddenlyUpdatedNodesUpdated(parent);
            }
        }
    }
}
