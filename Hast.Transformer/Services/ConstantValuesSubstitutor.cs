using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hast.Transformer.Helpers;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;
using Mono.Cecil;
using Orchard.Validation;

namespace Hast.Transformer.Services
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


        public IArraySizeHolder SubstituteConstantValues(SyntaxTree syntaxTree)
        {
            // Gradually propagating the constant values through the syntax tree so this needs multiple passes. So 
            // running them until nothing changes.

            var typeDeclarationLookupTable = _typeDeclarationLookupTableFactory.Create(syntaxTree);
            var arraySizeHolder = new ArraySizeHolder();
            var constantValuesTable = new ConstantValuesTable();

            var constantValuesMarkingVisitor = new ConstantValuesMarkingVisitor(
                constantValuesTable, typeDeclarationLookupTable, _astExpressionEvaluator, arraySizeHolder);
            var globalValueHoldersHandlingVisitor =
                new GlobalValueHoldersHandlingVisitor(constantValuesTable, typeDeclarationLookupTable, syntaxTree);
            var constantValuesSubstitutingVisitor =
                new ConstantValuesSubstitutingVisitor(constantValuesTable, arraySizeHolder, typeDeclarationLookupTable);

            string codeOutput;
            var updatedArraysCount = 0;
            var passCount = 0;
            const int maxPassCount = 1000;
            do
            {
                codeOutput = syntaxTree.ToString();
                updatedArraysCount = constantValuesMarkingVisitor.ArraySizesUpdated.Count;

                syntaxTree.AcceptVisitor(constantValuesMarkingVisitor);
                syntaxTree.AcceptVisitor(globalValueHoldersHandlingVisitor);
                syntaxTree.AcceptVisitor(constantValuesSubstitutingVisitor);

                constantValuesTable.Clear();

                passCount++;
            } while ((codeOutput != syntaxTree.ToString() || constantValuesMarkingVisitor.ArraySizesUpdated.Count != updatedArraysCount) &&
                    passCount < maxPassCount);

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

            public HashSet<string> ArraySizesUpdated { get; }


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

                ArraySizesUpdated = new HashSet<string>();
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
                    _constantValuesTable.MarkAsPotentiallyConstant(
                        assignmentExpression.Left,
                        primitiveExpression,
                        primitiveExpressionParent.FindFirstParentBlockStatement());
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
                        _constantValuesTable.MarkAsPotentiallyConstant(
                            binaryOperatorExpression,
                            newExpression,
                            parentBlock);

                        AssignmentExpression assignment;
                        if (binaryOperatorExpression.Parent.Is<AssignmentExpression>(out assignment))
                        {
                            _constantValuesTable.MarkAsNonConstant(assignment.Left, parentBlock);
                        }
                    }
                }

                // ObjectCreateExpression, ReturnStatement, InvocationExpression are handled in GlobalValueHoldersHandlingVisitor.
            }


            protected override void VisitChildren(AstNode node)
            {
                base.VisitChildren(node);

                if ((!(node is IdentifierExpression) &&
                    (!(node is MemberReferenceExpression) || ConstantValueSubstitutionHelper.IsMethodInvocation((MemberReferenceExpression)node))) ||
                    node.GetActualTypeReference()?.IsArray == false)
                {
                    return;
                }

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


            private void PassLengthOfArrayHolderToParent(AstNode arrayHolder, int arrayLength)
            {
                var parent = arrayHolder.Parent;

                Action<AstNode> updateArraySizesUpdated = node => ArraySizesUpdated.Add(node.GetFullName());

                AssignmentExpression assignmentExpression;

                if (parent.Is<AssignmentExpression>(out assignmentExpression))
                {
                    if (assignmentExpression.Left is MemberReferenceExpression)
                    {
                        _arraySizeHolder.SetSize(
                            ((MemberReferenceExpression)assignmentExpression.Left).FindMemberDeclaration(_typeDeclarationLookupTable),
                            arrayLength);
                    }
                    _arraySizeHolder.SetSize(assignmentExpression.Left, arrayLength);
                    updateArraySizesUpdated(assignmentExpression.Left);
                }
                else if (parent is InvocationExpression)
                {
                    var parameter = ConstantValueSubstitutionHelper.FindMethodParameterForPassedExpression(
                        (InvocationExpression)parent,
                        (Expression)arrayHolder,
                        _typeDeclarationLookupTable);

                    // The parameter can be null for special invocations like Task.WhenAll().
                    if (parameter == null) return;

                    _arraySizeHolder.SetSize(parameter, arrayLength);
                    updateArraySizesUpdated(parameter);
                }
                else if (parent is ObjectCreateExpression)
                {
                    var parameter = ConstantValueSubstitutionHelper.FindConstructorParameterForPassedExpression(
                            (ObjectCreateExpression)parent,
                            (Expression)arrayHolder,
                            _typeDeclarationLookupTable);

                    _arraySizeHolder.SetSize(parameter, arrayLength);
                    updateArraySizesUpdated(parameter);
                }
                else if (parent is VariableInitializer)
                {
                    _arraySizeHolder.SetSize(parent, arrayLength);
                    updateArraySizesUpdated(parent);
                }
            }
        }

        /// <summary>
        /// The value of parameters of an object creation or method invocation, a member (in this case: field or 
        /// property) or what's returned from a method can only be substituted if they have a globally unique value, 
        /// since these are used not just from a single method (in contrast to variables). Thus these need special
        /// care, handling them here.
        /// </summary>
        private class GlobalValueHoldersHandlingVisitor : DepthFirstAstVisitor
        {
            private readonly ConstantValuesTable _constantValuesTable;
            private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;
            private readonly SyntaxTree _syntaxTree;


            public GlobalValueHoldersHandlingVisitor(
                ConstantValuesTable constantValuesTable,
                ITypeDeclarationLookupTable typeDeclarationLookupTable,
                SyntaxTree syntaxTree)
            {
                _constantValuesTable = constantValuesTable;
                _typeDeclarationLookupTable = typeDeclarationLookupTable;
                _syntaxTree = syntaxTree;
            }


            public override void VisitObjectCreateExpression(ObjectCreateExpression objectCreateExpression)
            {
                base.VisitObjectCreateExpression(objectCreateExpression);

                // Marking all parameters where a non-constant parameter is passed as non-constant.
                foreach (var argument in objectCreateExpression.Arguments)
                {
                    var parameter = ConstantValueSubstitutionHelper
                        .FindConstructorParameterForPassedExpression(objectCreateExpression, argument, _typeDeclarationLookupTable);

                    if (argument is PrimitiveExpression)
                    {
                        _constantValuesTable.MarkAsPotentiallyConstant(parameter, (PrimitiveExpression)argument, _syntaxTree, true);
                    }
                    else
                    {
                        _constantValuesTable.MarkAsNonConstant(parameter, _syntaxTree);
                    }
                }
            }

            public override void VisitInvocationExpression(InvocationExpression invocationExpression)
            {
                base.VisitInvocationExpression(invocationExpression);

                // Marking all parameters where a non-constant parameter is passed as non-constant.
                foreach (var argument in invocationExpression.Arguments)
                {
                    var parameter = ConstantValueSubstitutionHelper
                        .FindMethodParameterForPassedExpression(invocationExpression, argument, _typeDeclarationLookupTable);

                    if (argument is PrimitiveExpression)
                    {
                        _constantValuesTable.MarkAsPotentiallyConstant(parameter, (PrimitiveExpression)argument, _syntaxTree, true);
                    }
                    else
                    {
                        _constantValuesTable.MarkAsNonConstant(parameter, _syntaxTree);
                    }
                }
            }

            public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            {
                base.VisitMemberReferenceExpression(memberReferenceExpression);

                AssignmentExpression parentAssignment;

                // We only care about cases where this member is assigned to.
                if (!memberReferenceExpression.Parent.Is(assignment => assignment.Left == memberReferenceExpression, out parentAssignment))
                {
                    return;
                }

                var memberEntity = memberReferenceExpression.FindMemberDeclaration(_typeDeclarationLookupTable);

                // If a primitive value is assigned then re-mark it so if there are multiple different such assignments
                // then the member will be unmarked.
                if (parentAssignment.Right is PrimitiveExpression)
                {
                    _constantValuesTable
                        .MarkAsPotentiallyConstant(memberEntity, (PrimitiveExpression)parentAssignment.Right, _syntaxTree, true);
                }
                else
                {
                    _constantValuesTable.MarkAsNonConstant(memberEntity, _syntaxTree);
                }
            }

            public override void VisitReturnStatement(ReturnStatement returnStatement)
            {
                base.VisitReturnStatement(returnStatement);

                // Method substitution is only valid if there is only one return statement in the method (or multiple
                // ones but returning the same constant value).
                if (returnStatement.Expression is PrimitiveExpression)
                {
                    _constantValuesTable.MarkAsPotentiallyConstant(
                        returnStatement.FindFirstParentEntityDeclaration(),
                        (PrimitiveExpression)returnStatement.Expression,
                        _syntaxTree,
                        true);
                }
                else if (!returnStatement.Expression.GetFullName().IsBackingFieldName())
                {
                    _constantValuesTable.MarkAsNonConstant(returnStatement.FindFirstParentEntityDeclaration(), _syntaxTree);
                }
            }

            // Since fields and properties can be read even without initializing them they can be only substituted if 
            // they are read-only (or just have their data type defaults assigned to them). This is possible with 
            // readonly fields and auto-properties having only getters.
            // So to prevent anything else from being substituted marking those as non-constant (this could be improved
            // to still substitute the .NET default value if nothing else is assigned, but this should be exteremly rare).
            // With the scope-using substitution this restriction seems to be safe to remove, however since with read-
            // only memmbers potentially constant values are also enforced in .NET it's safer to leave it like this.

            public override void VisitPropertyDeclaration(PropertyDeclaration propertyDeclaration)
            {
                base.VisitPropertyDeclaration(propertyDeclaration);

                if (propertyDeclaration.Setter != Accessor.Null)
                {
                    _constantValuesTable.MarkAsNonConstant(propertyDeclaration, _syntaxTree);
                }
            }

            public override void VisitFieldDeclaration(FieldDeclaration fieldDeclaration)
            {
                base.VisitFieldDeclaration(fieldDeclaration);

                if (!fieldDeclaration.HasModifier(Modifiers.Readonly))
                {
                    _constantValuesTable.MarkAsNonConstant(fieldDeclaration, _syntaxTree);
                }
            }
        }

        private class ConstantValuesSubstitutingVisitor : DepthFirstAstVisitor
        {
            private readonly ConstantValuesTable _constantValuesTable;
            private readonly IArraySizeHolder _arraySizeHolder;
            private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;


            public ConstantValuesSubstitutingVisitor(
                ConstantValuesTable constantValuesTable,
                IArraySizeHolder arraySizeHolder,
                ITypeDeclarationLookupTable typeDeclarationLookupTable)
            {
                _constantValuesTable = constantValuesTable;
                _arraySizeHolder = arraySizeHolder;
                _typeDeclarationLookupTable = typeDeclarationLookupTable;
            }


            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);

                // If this is assignment is in a while or an if-else then every assignment to it shouldn't affect
                // anything in the outer scope after this ("after this" works because the visitor visits nodes in
                // topological order). Neither if this is assigning a non-constant value.

                if (!(assignmentExpression.Left is IdentifierExpression)) return;

                if (ConstantValueSubstitutionHelper.IsInWhile(assignmentExpression) ||
                    assignmentExpression.IsIn<IfElseStatement>())
                {
                    _constantValuesTable.MarkAsNonConstant(
                        assignmentExpression.Left,
                        // The first block will be the if-else or the while statement.
                        assignmentExpression.FindFirstParentBlockStatement().FindFirstParentBlockStatement());
                }
                else if (!(assignmentExpression.Right is PrimitiveExpression) &&
                    !assignmentExpression.Right.Is<BinaryOperatorExpression>(binary =>
                        binary.Left.GetFullName() == assignmentExpression.Left.GetFullName() &&
                            binary.Right is PrimitiveExpression ||
                        binary.Right.GetFullName() == assignmentExpression.Left.GetFullName() &&
                            binary.Left is PrimitiveExpression))
                {
                    _constantValuesTable.MarkAsNonConstant(
                        assignmentExpression.Left,
                        assignmentExpression.FindFirstParentBlockStatement());
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
                if (ConstantValueSubstitutionHelper.IsMethodInvocation(memberReferenceExpression)) return;

                if (memberReferenceExpression.IsArrayLengthAccess())
                {
                    var arraySize = _arraySizeHolder.GetSize(memberReferenceExpression.Target);

                    if (arraySize == null && memberReferenceExpression.Target is MemberReferenceExpression)
                    {
                        arraySize = _arraySizeHolder.GetSize(
                            ((MemberReferenceExpression)memberReferenceExpression.Target).FindMemberDeclaration(_typeDeclarationLookupTable));
                    }

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
                    ConstantValueSubstitutionHelper.IsInWhile(binaryOperatorExpression))
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

                if (ConstantValueSubstitutionHelper.IsInWhile(ifElseStatement)) return;

                var primitiveCondition = ifElseStatement.Condition as PrimitiveExpression;

                if (primitiveCondition == null) return;

                Action<Statement> replaceIfElse = branchStatement =>
                {
                    // Moving all statements from the block up.
                    if (branchStatement is BlockStatement)
                    {
                        foreach (var statement in branchStatement.Children)
                        {
                            AstInsertionHelper.InsertStatementBefore(ifElseStatement, (Statement)statement.Clone());
                        }
                    }
                    else ifElseStatement.ReplaceWith(branchStatement.Clone());
                };

                if (primitiveCondition.Value.Equals(true)) replaceIfElse(ifElseStatement.TrueStatement);
                else if (ifElseStatement.FalseStatement != Statement.Null) replaceIfElse(ifElseStatement.FalseStatement);

                ifElseStatement.Remove();
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
                    ConstantValueSubstitutionHelper.IsInWhile(expression))
                {
                    return;
                }

                PrimitiveExpression valueExpression;
                // First checking if there is a substitution for the expression; if not then if it's a member reference
                // then check whether there is a global substitution for the member.
                if (_constantValuesTable.RetrieveAndDeleteConstantValue(expression, out valueExpression) ||
                    expression.Is<MemberReferenceExpression>(memberReferenceExpression =>
                    {
                        var member = memberReferenceExpression.FindMemberDeclaration(_typeDeclarationLookupTable);
                        if (member == null) return false;
                        return _constantValuesTable.RetrieveAndDeleteConstantValue(member, out valueExpression);
                    }))
                {
                    expression.ReplaceWith(valueExpression.Clone());
                }
            }
        }


        private class ConstantValuesTable
        {
            // The outer dictionary is keyed by value holder names. In the inner dictionary the scope is the key and 
            // the value is the primitive value.
            private readonly Dictionary<string, Dictionary<AstNode, PrimitiveExpression>> _valueHoldersAndValueDescriptors =
                new Dictionary<string, Dictionary<AstNode, PrimitiveExpression>>();


            /// <param name="scope">The node within the value should valid.</param>
            public void MarkAsPotentiallyConstant(
                AstNode valueHolder,
                PrimitiveExpression expression,
                AstNode scope,
                bool disallowDifferentValues = false)
            {
                Argument.ThrowIfNull(scope, nameof(scope));

                if (valueHolder == null) return;

                var valueDescriptors = GetOrCreateValueDescriptors(valueHolder.GetFullNameWithUnifiedPropertyName());

                if (disallowDifferentValues && expression != null)
                {
                    PrimitiveExpression existingExpression;
                    if (valueDescriptors.TryGetValue(scope, out existingExpression))
                    {
                        // Simply using != would yield a reference equality check.
                        if (existingExpression == null || !expression.Value.Equals(existingExpression.Value))
                        {
                            expression = null;
                        }
                    }
                }

                valueDescriptors[scope] = expression;
            }

            public void MarkAsNonConstant(AstNode valueHolder, AstNode scope)
            {
                if (valueHolder == null) return;

                Argument.ThrowIfNull(scope, nameof(scope));

                GetOrCreateValueDescriptors(valueHolder.GetFullNameWithUnifiedPropertyName())[scope] = null;
            }

            public bool RetrieveAndDeleteConstantValue(AstNode valueHolder, out PrimitiveExpression valueExpression)
            {
                Dictionary<AstNode, PrimitiveExpression> valueDescriptors;

                if (_valueHoldersAndValueDescriptors.TryGetValue(valueHolder.GetFullNameWithUnifiedPropertyName(), out valueDescriptors) &&
                    valueDescriptors.Any())
                {
                    // Finding the value defined for the scope which is closest.
                    var closestValueDescriptorWithHeight = valueDescriptors
                        .Select(valueDescriptor =>
                        {
                            int height;
                            var parent = valueHolder.FindFirstParentOfType((AstNode node) => node == valueDescriptor.Key, out height);

                            return new
                            {
                                ValueDescriptor = valueDescriptor,
                                Height = parent != null ? height : int.MaxValue
                            };
                        })
                        .OrderBy(valueWithHeight => valueWithHeight.Height)
                        .First();

                    valueExpression = closestValueDescriptorWithHeight.ValueDescriptor.Value;
                    var suitableValueFound = valueExpression != null && closestValueDescriptorWithHeight.Height != int.MaxValue;
                    if (suitableValueFound) valueDescriptors.Clear();
                    return suitableValueFound;
                }

                valueExpression = null;
                return false;
            }

            public void Clear() => _valueHoldersAndValueDescriptors.Clear();


            private Dictionary<AstNode, PrimitiveExpression> GetOrCreateValueDescriptors(string holderName)
            {
                Dictionary<AstNode, PrimitiveExpression> valueDescriptors;

                if (!_valueHoldersAndValueDescriptors.TryGetValue(holderName, out valueDescriptors))
                {
                    valueDescriptors = new Dictionary<AstNode, PrimitiveExpression>();
                    _valueHoldersAndValueDescriptors[holderName] = valueDescriptors;
                }

                return valueDescriptors;
            }
        }
    }
}
