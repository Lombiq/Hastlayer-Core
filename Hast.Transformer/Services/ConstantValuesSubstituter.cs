using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Orchard.Validation;

namespace Hast.Transformer.Services
{
    public class ConstantValuesSubstituter : IConstantValuesSubstituter
    {
        private readonly ITypeDeclarationLookupTableFactory _typeDeclarationLookupTableFactory;
        private readonly IAstExpressionEvaluator _astExpressionEvaluator;


        public ConstantValuesSubstituter(
            ITypeDeclarationLookupTableFactory typeDeclarationLookupTableFactory,
            IAstExpressionEvaluator astExpressionEvaluator)
        {
            _typeDeclarationLookupTableFactory = typeDeclarationLookupTableFactory;
            _astExpressionEvaluator = astExpressionEvaluator;
        }


        public IArraySizeHolder SubstituteConstantValues(SyntaxTree syntaxTree)
        {
            var typeDeclarationLookupTable = _typeDeclarationLookupTableFactory.Create(syntaxTree);

            // Gradually propagating the constant values through the syntax tree so this needs multiple passes. So 
            // running them until nothing changes.

            var arraySizeHolder = new ArraySizeHolder();

            string codeOutput;
            var passCount = 0;
            const int maxPassCount = 1000;
            do
            {
                codeOutput = syntaxTree.ToString();

                var constantValuesTable = new ConstantValuesTable();

                syntaxTree.AcceptVisitor(new ConstantValuesMarkingVisitor(
                    constantValuesTable, typeDeclarationLookupTable, _astExpressionEvaluator, arraySizeHolder));
                syntaxTree.AcceptVisitor(new ConstantValuesSubstitutingVisitor(constantValuesTable, arraySizeHolder));

                passCount++;
            } while (codeOutput != syntaxTree.ToString() && passCount < maxPassCount);

            if (passCount == maxPassCount)
            {
                throw new InvalidOperationException(
                    "Constant substitution needs more than " + maxPassCount +
                    "passes through the syntax tree. This most possibly indicates some error or the assembly being processed is exceptionally big.");
            }

            return arraySizeHolder;
        }


        private class ConstantValuesMarkingVisitor : DepthFirstAstVisitor
        {
            private readonly ConstantValuesTable _constantValuesTable;
            private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;
            private readonly IAstExpressionEvaluator _astExpressionEvaluator;
            private readonly IArraySizeHolder _arraySizeHolder;


            public ConstantValuesMarkingVisitor(
                ConstantValuesTable constantValuesTable,
                ITypeDeclarationLookupTable typeDeclarationLookupTable,
                IAstExpressionEvaluator astExpressionEvaluator,
                IArraySizeHolder arraySizeHolder)
            {
                _constantValuesTable = constantValuesTable;
                _typeDeclarationLookupTable = typeDeclarationLookupTable;
                _astExpressionEvaluator = astExpressionEvaluator;
                _arraySizeHolder = arraySizeHolder;
            }


            public override void VisitPrimitiveExpression(PrimitiveExpression primitiveExpression)
            {
                base.VisitPrimitiveExpression(primitiveExpression);

                var primitiveExpressionParent = primitiveExpression.Parent;

                if (primitiveExpressionParent is ICSharpCode.NRefactory.CSharp.Attribute) return;

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
                InvocationExpression invocationExpression;
                ArrayCreateExpression arrayCreateExpression;
                BinaryOperatorExpression binaryOperatorExpression;
                ObjectCreateExpression objectCreateExpression;
                ReturnStatement returnStatement;


                // Don't substitute if this is inside an if or while statement.
                if (primitiveExpressionParent.Is<AssignmentExpression>(out assignmentExpression) &&
                    !primitiveExpression.IsIn<IfElseStatement>() &&
                    !primitiveExpression.IsIn<WhileStatement>())
                {
                    _constantValuesTable.MarkAsPotentiallyConstant(assignmentExpression.Left, primitiveExpression);
                }
                else if (primitiveExpressionParent.Is<InvocationExpression>(out invocationExpression))
                {
                    _constantValuesTable.MarkAsPotentiallyConstant(
                            FindMethodParameterForPassedExpression(
                                invocationExpression,
                                primitiveExpression),
                            primitiveExpression,
                            true);
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
                        if (resultType.FullName == typeof(bool).FullName)
                        {
                            newExpression.Value = newExpression.Value.ToString() == 1.ToString();
                        }

                        _constantValuesTable.MarkAsPotentiallyConstant(binaryOperatorExpression, newExpression);

                        AssignmentExpression assignment;
                        if (binaryOperatorExpression.Parent.Is<AssignmentExpression>(out assignment))
                        {
                            _constantValuesTable.MarkAsNonConstant(assignment.Left);
                        }
                    }
                }
                else if (primitiveExpressionParent.Is<ObjectCreateExpression>(out objectCreateExpression))
                {
                    var parameter = FindConstructorParameterForPassedExpression(objectCreateExpression, primitiveExpression);

                    _constantValuesTable.MarkAsPotentiallyConstant(parameter, primitiveExpression, true);
                }
                else if (primitiveExpressionParent.Is<ReturnStatement>(out returnStatement) &&
                    returnStatement.Expression == primitiveExpression)
                {
                    _constantValuesTable.MarkAsPotentiallyConstant(
                        primitiveExpression.FindFirstParentEntityDeclaration(),
                        primitiveExpression,
                        true);
                }
            }

            public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
            {
                base.VisitObjectCreateExpression(objectCreateExpression);

                // Marking all parameters where a non-constant parameter is passed as non-constant.
                foreach (var argument in objectCreateExpression.Arguments)
                {
                    if (!(argument is PrimitiveExpression))
                    {
                        _constantValuesTable.MarkAsNonConstant(FindConstructorParameterForPassedExpression(objectCreateExpression, argument));
                    }
                }
            }

            public override void VisitInvocationExpression(InvocationExpression invocationExpression)
            {
                base.VisitInvocationExpression(invocationExpression);

                // Marking all parameters where a non-constant parameter is passed as non-constant.
                foreach (var argument in invocationExpression.Arguments)
                {
                    if (!(argument is PrimitiveExpression))
                    {
                        _constantValuesTable.MarkAsNonConstant(FindMethodParameterForPassedExpression(invocationExpression, argument));
                    }
                }
            }

            protected override void VisitChildren(AstNode node)
            {
                base.VisitChildren(node);

                if ((!(node is IdentifierExpression) &&
                    !(node is MemberReferenceExpression)) ||
                    node.GetActualTypeReference()?.IsArray == false)
                {
                    return;
                }

                var existingSize = _arraySizeHolder.GetSize(node);

                if (existingSize == null) return;

                PassLengthOfArrayHolderToParent(node, existingSize.Length);
            }

            private ParameterDeclaration FindConstructorParameterForPassedExpression(
                ObjectCreateExpression objectCreateExpression,
                Expression passedExpression) =>
                FindParameterForExpressionPassedToCall(objectCreateExpression, objectCreateExpression.Arguments, passedExpression);

            private ParameterDeclaration FindMethodParameterForPassedExpression(
                InvocationExpression invocationExpression,
                Expression passedExpression) =>
                FindParameterForExpressionPassedToCall(invocationExpression, invocationExpression.Arguments, passedExpression);

            // This could be optimized not to look up everything every time when called from VisitObjectCreateExpression()
            // and VisitInvocationExpression().
            private ParameterDeclaration FindParameterForExpressionPassedToCall(
                Expression callExpression,
                AstNodeCollection<Expression> invocationArguments,
                Expression passedExpression)
            {
                var methodDefinition = callExpression.Annotation<MethodDefinition>();

                if (methodDefinition == null) return null;

                var parameterSimpleName = methodDefinition
                    .Parameters[invocationArguments.ToList().FindIndex(argumentExpression => argumentExpression == passedExpression)]
                    .Name;

                var constructorFullName = callExpression.GetFullName();

                return ((MethodDeclaration)_typeDeclarationLookupTable
                    .Lookup(methodDefinition.DeclaringType.FullName)
                    .Members
                    .Single(member => member.GetFullName() == constructorFullName))
                    .Parameters
                    .Single(p => p.Name == parameterSimpleName);
            }

            private void PassLengthOfArrayHolderToParent(AstNode arrayHolder, int arrayLength)
            {
                var parent = arrayHolder.Parent;

                AssignmentExpression assignmentExpression;

                if (parent.Is<AssignmentExpression>(out assignmentExpression))
                {
                    _arraySizeHolder.SetSize(assignmentExpression.Left, arrayLength);
                }
                else if (parent is InvocationExpression)
                {
                    _arraySizeHolder.SetSize(
                        FindMethodParameterForPassedExpression((InvocationExpression)parent, (Expression)arrayHolder),
                        arrayLength);
                }
                else if (parent is ObjectCreateExpression)
                {
                    _arraySizeHolder.SetSize(
                        FindConstructorParameterForPassedExpression((ObjectCreateExpression)parent, (Expression)arrayHolder),
                        arrayLength);
                }
            }
        }

        private class ConstantValuesSubstitutingVisitor : DepthFirstAstVisitor
        {
            private readonly ConstantValuesTable _constantValuesTable;
            private readonly IArraySizeHolder _arraySizeHolder;


            public ConstantValuesSubstitutingVisitor(
                ConstantValuesTable constantValuesTable, IArraySizeHolder arraySizeHolder)
            {
                _constantValuesTable = constantValuesTable;
                _arraySizeHolder = arraySizeHolder;
            }


            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);

                // If this is assignment is in a while or an if-else then every assignment to it shouldn't affect
                // anything in the outer scope after this. Neither if this is assigning a non-constant value.
                // This could eventually be made more sophisticated by taking care of variable scopes too, so these
                // assignments would affect variables inside the scope still.
                if ((assignmentExpression.Left is IdentifierExpression || assignmentExpression.Left is MemberReferenceExpression) &&
                    (IsInWhile(assignmentExpression) || assignmentExpression.IsIn<IfElseStatement>() || !(assignmentExpression.Right is PrimitiveExpression)))
                {
                    _constantValuesTable.MarkAsNonConstant(assignmentExpression.Left);
                }
            }

            public override void VisitReturnStatement(ReturnStatement returnStatement)
            {
                base.VisitReturnStatement(returnStatement);

                // Method substitution is only valid if there is only one return statement in the method so unmarking 
                // if there are more.
                if (!(returnStatement.Expression is PrimitiveExpression))
                {
                    _constantValuesTable.MarkAsNonConstant(returnStatement.FindFirstParentEntityDeclaration());
                }
                else
                {
                    // If this is not the same value as marked previously in ConstantValuesMarkingVisitor then the
                    // method will be unmarked.
                    _constantValuesTable.MarkAsPotentiallyConstant(
                        returnStatement.FindFirstParentEntityDeclaration(),
                        (PrimitiveExpression)returnStatement.Expression,
                        true);
                }
            }

            public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
            {
                base.VisitIdentifierExpression(identifierExpression);

                SubstituteValueHolderInExpressionIfInSuitableAssignment(identifierExpression);
            }

            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);

                // If this is a member reference to a method then nothing to do.
                if (memberReferenceExpression.Parent.Is<InvocationExpression>(invocation => invocation.Target == memberReferenceExpression))
                {
                    return;
                }

                if (memberReferenceExpression.IsArrayLengthAccess())
                {
                    var arraySize = _arraySizeHolder.GetSize(memberReferenceExpression.Target);

                    if (arraySize != null)
                    {
                        var newExpression = new PrimitiveExpression(arraySize.Length);
                        var typeInformation = memberReferenceExpression.Annotation<TypeInformation>();
                        if (typeInformation != null)
                        {
                            newExpression.AddAnnotation(typeInformation);
                        }
                        memberReferenceExpression.ReplaceWith(newExpression);
                    }

                    return;
                }

                SubstituteValueHolderInExpressionIfInSuitableAssignment(memberReferenceExpression);
            }

            public override void VisitBinaryOperatorExpression(BinaryOperatorExpression binaryOperatorExpression)
            {
                base.VisitBinaryOperatorExpression(binaryOperatorExpression);

                if (binaryOperatorExpression.FindFirstParentOfType<AttributeSection>() != null ||
                    IsInWhile(binaryOperatorExpression))
                {
                    return;
                }

                PrimitiveExpression valueExpression;
                if (_constantValuesTable.RetrieveAndDeleteConstantValue(binaryOperatorExpression, out valueExpression))
                {
                    binaryOperatorExpression.ReplaceWith(valueExpression.Clone());
                }
            }

            public override void VisitIfElseStatement(IfElseStatement ifElseStatement)
            {
                base.VisitIfElseStatement(ifElseStatement);

                if (IsInWhile(ifElseStatement)) return;

                var primitiveCondition = ifElseStatement.Condition as PrimitiveExpression;

                if (primitiveCondition == null) return;

                if (primitiveCondition.Value.Equals(true))
                {
                    ifElseStatement.ReplaceWith(ifElseStatement.TrueStatement.Clone());
                }
                else
                {
                    if (ifElseStatement.FalseStatement != Statement.Null)
                    {
                        ifElseStatement.ReplaceWith(ifElseStatement.FalseStatement.Clone());
                    }
                    else
                    {
                        ifElseStatement.Remove();
                    }
                }
            }

            public override void VisitInvocationExpression(InvocationExpression invocationExpression)
            {
                base.VisitInvocationExpression(invocationExpression);

                // Substituting method invocations that have a constant return value.
                SubstituteValueHolderInExpressionIfInSuitableAssignment(invocationExpression);
            }


            private void SubstituteValueHolderInExpressionIfInSuitableAssignment(Expression expression)
            {
                // If this is an value holder on the left side of an assignment then nothing to do. If it's in a while
                // statement then it can't be safely substituted (due to e.g. loop variables).
                if (expression.Parent.Is<AssignmentExpression>(assignment => assignment.Left == expression) ||
                    IsInWhile(expression))
                {
                    return;
                }

                PrimitiveExpression valueExpression;
                if (_constantValuesTable.RetrieveAndDeleteConstantValue(expression, out valueExpression))
                {
                    expression.ReplaceWith(valueExpression.Clone());
                }
            }


            private static bool IsInWhile(AstNode node) => node.IsIn<WhileStatement>();
        }


        private class ConstantValuesTable
        {
            private readonly Dictionary<string, PrimitiveExpression> _constantValuedVariablesAndMembers =
                new Dictionary<string, PrimitiveExpression>();


            public void MarkAsPotentiallyConstant(
                AstNode valueHolder,
                PrimitiveExpression expression,
                bool disallowDifferentValues = false)
            {
                if (valueHolder == null) return;

                Action<string> saveMark = name =>
                {
                    if (disallowDifferentValues)
                    {

                        PrimitiveExpression existingExpression;
                        if (_constantValuedVariablesAndMembers.TryGetValue(name, out existingExpression) &&
                            existingExpression != null)
                        {
                            // Simply using != would yield a reference equality check.
                            if (!expression.Value.Equals(existingExpression.Value)) expression = null;
                        }
                    }

                    _constantValuedVariablesAndMembers[name] = expression;
                };

                var holderName = valueHolder.GetFullName();

                saveMark(holderName);

                if (holderName.IsBackingFieldName()) saveMark(holderName.ConvertFullBackingFieldNameToPropertyName());
            }

            public void MarkAsNonConstant(AstNode valueHolder)
            {
                if (valueHolder == null) return;

                var holderName = valueHolder.GetFullName();
                _constantValuedVariablesAndMembers[holderName] = null;
                if (holderName.IsBackingFieldName())
                {
                    _constantValuedVariablesAndMembers[holderName.ConvertFullBackingFieldNameToPropertyName()] = null;
                }
            }

            public bool RetrieveAndDeleteConstantValue(AstNode valueHolder, out PrimitiveExpression valueExpression)
            {
                var holderName = valueHolder.GetFullName();

                if (_constantValuedVariablesAndMembers.TryGetValue(holderName, out valueExpression))
                {
                    _constantValuedVariablesAndMembers.Remove(holderName);
                    return valueExpression != null;
                }

                return false;
            }
        }
    }
}
