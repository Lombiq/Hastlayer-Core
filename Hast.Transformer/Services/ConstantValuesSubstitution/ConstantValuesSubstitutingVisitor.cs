using System;
using System.Linq;
using Hast.Transformer.Helpers;
using Hast.Transformer.Models;
using ICSharpCode.Decompiler.Ast;
using ICSharpCode.NRefactory.CSharp;

namespace Hast.Transformer.Services.ConstantValuesSubstitution
{
    internal class ConstantValuesSubstitutingVisitor : DepthFirstAstVisitor
    {
        private readonly ConstantValuesSubstitutingAstProcessor _constantValuesSubstitutingAstProcessor;
        private readonly ConstantValuesTable _constantValuesTable;
        private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;
        private readonly IArraySizeHolder _arraySizeHolder;


        public ConstantValuesSubstitutingVisitor(ConstantValuesSubstitutingAstProcessor constantValuesSubstitutingAstProcessor)
        {
            _constantValuesSubstitutingAstProcessor = constantValuesSubstitutingAstProcessor;
            _constantValuesTable = constantValuesSubstitutingAstProcessor.ConstantValuesTable;
            _typeDeclarationLookupTable = constantValuesSubstitutingAstProcessor.TypeDeclarationLookupTable;
            _arraySizeHolder = constantValuesSubstitutingAstProcessor.ArraySizeHolder;
        }


        public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
        {
            base.VisitAssignmentExpression(assignmentExpression);

            // Assignments should be handled here because those should only take effect "after this line" ("after this" 
            // works because the visitor visits nodes in topological order and thus possible substitutions will come 
            // after this method).

            var parentBlock = assignmentExpression.FindFirstParentBlockStatement();

            // Indexed assignments with a constant index could also be handled eventually, but not really needed
            // now.
            if (assignmentExpression.Right is PrimitiveExpression && !(assignmentExpression.Left is IndexerExpression))
            {
                // There won't be a parent block for attributes for example.
                if (parentBlock != null)
                {
                    _constantValuesSubstitutingAstProcessor.ConstantValuesTable.MarkAsPotentiallyConstant(
                        assignmentExpression.Left,
                        (PrimitiveExpression)assignmentExpression.Right,
                        parentBlock); 
                }
            }

            // If this is assignment is in a while or an if-else then every assignment to it shouldn't affect
            // anything in the outer scope after this. Neither if this is assigning a non-constant value.

            if (!(assignmentExpression.Left is IdentifierExpression)) return;

            if (ConstantValueSubstitutionHelper.IsInWhileOrIfElse(assignmentExpression))
            {
                // Finding all outer scopes. The current parentBlock block will be the if-else or the while statement itself.

                var currentParentBlock = parentBlock.FindFirstParentBlockStatement();

                while (currentParentBlock != null)
                {
                    _constantValuesTable.MarkAsNonConstant(assignmentExpression.Left, currentParentBlock);
                    currentParentBlock = currentParentBlock.FindFirstParentBlockStatement();
                }
            }
            else if (!(assignmentExpression.Right is PrimitiveExpression) &&
                !assignmentExpression.Right.Is<BinaryOperatorExpression>(binary =>
                    binary.Left.GetFullName() == assignmentExpression.Left.GetFullName() &&
                        binary.Right is PrimitiveExpression ||
                    binary.Right.GetFullName() == assignmentExpression.Left.GetFullName() &&
                        binary.Left is PrimitiveExpression))
            {
                _constantValuesTable.MarkAsNonConstant(assignmentExpression.Left,  parentBlock);
            }
        }

        public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
        {
            base.VisitIdentifierExpression(identifierExpression);

            // If it's used as a ref or out parameter in a method invocation then it can't be substituted after this line.
            if (identifierExpression.FindFirstNonParenthesizedExpressionParent() is DirectionExpression)
            {
                _constantValuesTable.MarkAsNonConstant(identifierExpression, identifierExpression.FindFirstParentBlockStatement());
            }

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

            if (_constantValuesTable.RetrieveAndDeleteConstantValue(binaryOperatorExpression, out var valueExpression))
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

            void replaceIfElse(Statement branchStatement)
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
            }

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

        public override void VisitUnaryOperatorExpression(UnaryOperatorExpression unaryOperatorExpression)
        {
            base.VisitUnaryOperatorExpression(unaryOperatorExpression);

            if (!(unaryOperatorExpression.Expression is PrimitiveExpression)) return;

            if (_constantValuesTable.RetrieveAndDeleteConstantValue(unaryOperatorExpression, out var valueExpression))
            {
                unaryOperatorExpression.ReplaceWith(valueExpression.Clone());
            }
        }

        public override void VisitArrayCreateExpression(ArrayCreateExpression arrayCreateExpression)
        {
            base.VisitArrayCreateExpression(arrayCreateExpression);

            if (arrayCreateExpression.Arguments.Count > 1)
            {
                ExceptionHelper.ThrowOnlySingleDimensionalArraysSupporterException(arrayCreateExpression);
            }

            var lengthArgument = arrayCreateExpression.Arguments.Single();
            var parentAssignment = arrayCreateExpression.Parent as AssignmentExpression;
            var existingSize = parentAssignment != null ?
                _arraySizeHolder.GetSize(parentAssignment.Left) :
                null;

            if (lengthArgument is PrimitiveExpression || existingSize == null) return;

            // If the array creation doesn't have a static length but the value holder the array is assigned to has the
            // array size defined then just substitute the array length too.
            lengthArgument.ReplaceWith(
                new PrimitiveExpression(existingSize.Length)
                .WithAnnotation(TypeHelper.CreateInt32TypeInformation()));
        }


        private void SubstituteValueHolderInExpressionIfInSuitableAssignment(Expression expression)
        {
            // If this is a value holder on the left side of an assignment then nothing to do. If it's in a while
            // statement then it can't be safely substituted (due to e.g. loop variables).
            if (expression.Parent.Is<AssignmentExpression>(assignment => assignment.Left == expression) ||
                ConstantValueSubstitutionHelper.IsInWhile(expression))
            {
                return;
            }

            // If the value holder is inside a unary operator that can mutate its state then it can't be substituted.
            var mutatingUnaryOperators = new[]
            {
                UnaryOperatorType.Decrement,
                UnaryOperatorType.Increment,
                UnaryOperatorType.PostDecrement,
                UnaryOperatorType.PostIncrement
            };
            if (expression.Parent.Is<UnaryOperatorExpression>(unary => mutatingUnaryOperators.Contains(unary.Operator)))
            {
                return;
            }

            // First checking if there is a substitution for the expression; if not then if it's a member reference
            // then check whether there is a global substitution for the member.
            if (_constantValuesTable.RetrieveAndDeleteConstantValue(expression, out PrimitiveExpression valueExpression) ||
                expression.Is<MemberReferenceExpression>(memberReferenceExpression =>
                {
                    var member = memberReferenceExpression.FindMemberDeclaration(_typeDeclarationLookupTable);

                    if (member == null) return false;

                    MethodDeclaration constructor = null;
                    if (_constantValuesTable.RetrieveAndDeleteConstantValue(member, out valueExpression))
                    {
                        return true;
                    }
                    else if (member.IsReadOnlyMember())
                    {
                        // If this is a nested member reference (e.g. _member.Property1.Property2) then let's find the
                        // first member that has a corresponding ctor.
                        var currentMemberReference = memberReferenceExpression;

                        while (
                            !_constantValuesSubstitutingAstProcessor.ObjectHoldersToConstructorsMappings
                                .TryGetValue(currentMemberReference.Target.GetFullName(), out constructor) &&
                            currentMemberReference.Target is MemberReferenceExpression)
                        {
                            currentMemberReference = (MemberReferenceExpression)currentMemberReference.Target;
                        }

                        if (constructor == null) return false;

                        // Try to substitute this member reference's value with a value set in the corresponding
                        // constructor.

                        // Trying to find a place where the same member is references on the same ("this") instance.
                        var memberReferenceExpressionInConstructor = ConstantValueSubstitutionHelper
                            .FindMemberReferenceInConstructor(constructor, member.GetFullName(), _typeDeclarationLookupTable);

                        if (memberReferenceExpressionInConstructor == null) return false;

                        // Using the substitution also used in the constructor. This should be safe to do even if
                        // in the ctor there are multiple assignments because an unretrieved constant will only
                        // remain in the ConstantValuesTable if there are no more substitutions needed in the ctor.
                        // But for this we need to rebuild a ConstantValuesTable just for this ctor. At this point the
                        // ctor should be fully substituted so we only need to care about primitive expressions.

                        var constructorConstantValuesTableBuildingVisitor =
                            new ConstructorConstantValuesTableBuildingVisitor(constructor);
                        constructor.AcceptVisitor(constructorConstantValuesTableBuildingVisitor);

                        return constructorConstantValuesTableBuildingVisitor.ConstantValuesTable
                            .RetrieveAndDeleteConstantValue(memberReferenceExpressionInConstructor, out valueExpression);
                    }

                    return false;
                }))
            {
                expression.ReplaceWith(valueExpression.Clone());
            }
        }


        private class ConstructorConstantValuesTableBuildingVisitor : DepthFirstAstVisitor
        {
            public ConstantValuesTable ConstantValuesTable { get; } = new ConstantValuesTable();

            private readonly MethodDeclaration _constructor;


            public ConstructorConstantValuesTableBuildingVisitor(MethodDeclaration constructor)
            {
                _constructor = constructor;
            }


            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);

                var right = assignmentExpression.Right as PrimitiveExpression;

                // We need to keep track of the last assignment in the root scope of the method. If after that there is
                // another assignment with a non-constant value or in an if-else or while then that makes the value 
                // holder's constant value unusable.

                if (right == null || ConstantValueSubstitutionHelper.IsInWhileOrIfElse(assignmentExpression))
                {
                    ConstantValuesTable.MarkAsNonConstant(assignmentExpression.Left, _constructor);
                }
                else if (right != null)
                {
                    ConstantValuesTable.MarkAsPotentiallyConstant(assignmentExpression.Left, right, _constructor);
                }
            }
        }
    }
}
