using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicExpresso;
using Hast.Transformer.Models;
using ICSharpCode.NRefactory.CSharp;
using Orchard.Validation;

namespace Hast.Transformer.Services
{
    public class ConstantValuesSubstituter : IConstantValuesSubstituter
    {
        private readonly ITypeDeclarationLookupTableFactory _typeDeclarationLookupTableFactory;


        public ConstantValuesSubstituter(ITypeDeclarationLookupTableFactory typeDeclarationLookupTableFactory)
        {
            _typeDeclarationLookupTableFactory = typeDeclarationLookupTableFactory;
        }


        public void SubstituteConstantValues(SyntaxTree syntaxTree)
        {
            var typeDeclarationLookupTable = _typeDeclarationLookupTableFactory.Create(syntaxTree);

            // Gradually propagating the constant values through the syntax tree so this needs multiple passes. So 
            // running them until nothing changes.

            string codeOutput;
            var passCount = 0;
            const int maxPassCount = 50;
            do
            {
                codeOutput = syntaxTree.ToString();

                var constantValuesTable = new ConstantValuesTable();

                var constantValuesMarkingVisitor = new ConstantValuesMarkingVisitor(
                    constantValuesTable, typeDeclarationLookupTable);
                syntaxTree.AcceptVisitor(constantValuesMarkingVisitor);
                syntaxTree.AcceptVisitor(new ConstantValuesSubstitutingVisitor(constantValuesTable));

                File.WriteAllText("source.cs", syntaxTree.ToString());
                passCount++;
            } while (codeOutput != syntaxTree.ToString() && passCount < maxPassCount);

            if (passCount == maxPassCount)
            {
                throw new InvalidOperationException(
                    "Constant substitution needs more than " + maxPassCount +
                    "passes through the syntax tree. This most possibly indicates some error or the assembly being processed is exceptionally big.");
            }
        }


        private class ConstantValuesMarkingVisitor : DepthFirstAstVisitor
        {
            private static Interpreter _interpreter = new Interpreter();

            private readonly ConstantValuesTable _constantValuesTable;
            private readonly ITypeDeclarationLookupTable _typeDeclarationLookupTable;


            public ConstantValuesMarkingVisitor(
                ConstantValuesTable constantValuesTable,
                ITypeDeclarationLookupTable typeDeclarationLookupTable)
            {
                _constantValuesTable = constantValuesTable;
                _typeDeclarationLookupTable = typeDeclarationLookupTable;
            }


            public override void VisitPrimitiveExpression(PrimitiveExpression primitiveExpression)
            {
                base.VisitPrimitiveExpression(primitiveExpression);

                var primitiveExpressionParent = primitiveExpression.Parent;

                if (primitiveExpressionParent is ICSharpCode.NRefactory.CSharp.Attribute) return;


                // The unnecessary type arguments for the Is<T> calls are left in because they'll be needed after
                // migrating to C# 7 and using out variables.
                IdentifierExpression identifierExpression = null;
                InvocationExpression invocationExpression;
                ArrayCreateExpression arrayCreateExpression;
                BinaryOperatorExpression binaryOperatorExpression;

                Func<AstNode, bool> handleAssignmentExpression = parent =>
                {
                    // Don't substitute if the value holder is not assigned to or if this is inside an if statement.
                    if (!parent.Is<AssignmentExpression>(assignment =>
                            assignment.Left.Is<IdentifierExpression>(out identifierExpression)))
                    {
                        return false;
                    }

                    if (primitiveExpression.IsIn<IfElseStatement>() || primitiveExpression.IsIn<WhileStatement>())
                    {
                        return false;
                    }

                    _constantValuesTable.MarkAsPotentiallyConstant(identifierExpression, primitiveExpression);

                    return true;
                };


                if (handleAssignmentExpression(primitiveExpressionParent)) return;

                if (primitiveExpressionParent.Is<InvocationExpression>(out invocationExpression))
                {
                    MemberReferenceExpression memberReferenceExpression;
                    if (invocationExpression.Is<MemberReferenceExpression>(out memberReferenceExpression))
                    {
                        var method = memberReferenceExpression.FindMemberDeclaration(_typeDeclarationLookupTable) as MethodDeclaration;

                        if (method != null)
                        {
                            System.Diagnostics.Debugger.Break();
                            //method.Parameters.Single(parameter => parameter.GetFullName() == invocationExpression.Arguments.Single(argument => argument.va))
                        }
                    }
                }
                else if (primitiveExpressionParent.Is<ArrayCreateExpression>(out arrayCreateExpression))
                {
                    //if (!handleAssignmentExpression(arrayCreateExpression.Parent))
                    //{
                    //    System.Diagnostics.Debugger.Break();
                    //}
                }
                else if (primitiveExpressionParent.Is<BinaryOperatorExpression>(out binaryOperatorExpression))
                {
                    var left = binaryOperatorExpression.Left;
                    var right = binaryOperatorExpression.Right;

                    var otherExpression = left == primitiveExpression ? right : left;

                    if (otherExpression is PrimitiveExpression)
                    {
                        // Due to some literal formats not being supported (like "u" for uints, see: 
                        // https://github.com/davideicardi/DynamicExpresso/issues/62) it's safer to cast everything. 
                        // But the "System.Int32" format is not supported, just "Int32"...

                        var expressionString =
                            "(" + left.GetActualTypeReference().FullName.Replace("System.", string.Empty) + ")" +
                            ((PrimitiveExpression)left).Value.ToString() +
                            " " + BinaryOperatorExpression.GetOperatorRole(binaryOperatorExpression.Operator).Token + " " +
                            "(" + right.GetActualTypeReference().FullName.Replace("System.", string.Empty) + ")" +
                            ((PrimitiveExpression)right).Value.ToString();

                        var newExpression = new PrimitiveExpression(_interpreter.Eval(expressionString));
                        var resultType = binaryOperatorExpression.GetResultTypeReference();
                        newExpression.AddAnnotation(resultType);
                        if (resultType.FullName == typeof(bool).FullName)
                        {
                            newExpression.Value = newExpression.Value.ToString() == 1.ToString();
                        }

                        _constantValuesTable.MarkAsPotentiallyConstant(binaryOperatorExpression, newExpression);

                        AssignmentExpression assignmentExpression;
                        if (binaryOperatorExpression.Parent.Is<AssignmentExpression>(out assignmentExpression))
                        {
                            _constantValuesTable.MarkAsNonConstant(assignmentExpression.Left);
                        }
                    }
                }
            }
        }

        private class ConstantValuesSubstitutingVisitor : DepthFirstAstVisitor
        {
            private readonly ConstantValuesTable _constantValuesTable;


            public ConstantValuesSubstitutingVisitor(ConstantValuesTable constantValuesTable)
            {
                _constantValuesTable = constantValuesTable;
            }


            public override void VisitAssignmentExpression(AssignmentExpression assignmentExpression)
            {
                base.VisitAssignmentExpression(assignmentExpression);

                // If this is assignment is in a while or an if-else then every assignment to it shouldn't affect
                // anything in the outer scope after this.
                // This could eventually be made more sophisticated by taking care of variable scopes too, so these
                // assignments would affect variables inside the scope still.
                if (assignmentExpression.Left is IdentifierExpression &&
                    (IsInWhile(assignmentExpression) || assignmentExpression.IsIn<IfElseStatement>()))
                {
                    _constantValuesTable.MarkAsNonConstant(assignmentExpression.Left);
                }
            }

            public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
            {
                base.VisitIdentifierExpression(identifierExpression);

                // If this is an identifier on the left side of an assignment then nothing to do. If it's in a while
                // statement then it can't be safely substituted (due to e.g. loop variables).
                if (identifierExpression.Parent.Is<AssignmentExpression>(assignment => assignment.Left == identifierExpression) ||
                    IsInWhile(identifierExpression))
                {
                    return;
                }

                PrimitiveExpression valueExpression;
                if (_constantValuesTable.RetrieveAndDeleteConstantValue(identifierExpression, out valueExpression))
                {
                    identifierExpression.ReplaceWith(valueExpression.Clone());
                }
            }

            //public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
            //{
            //    base.VisitMemberReferenceExpression(memberReferenceExpression);
            //}

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


            private static bool IsInWhile(AstNode node) => node.IsIn<WhileStatement>();
        }


        private class ConstantValuesTable
        {
            private readonly Dictionary<string, PrimitiveExpression> _constantValuedVariablesAndMembers =
                new Dictionary<string, PrimitiveExpression>();


            public void MarkAsPotentiallyConstant(AstNode valueHolder, PrimitiveExpression expression)
            {
                Argument.ThrowIfNull(expression, nameof(expression));

                _constantValuedVariablesAndMembers[valueHolder.GetFullName()] = expression;
            }

            public void MarkAsNonConstant(AstNode valueHolder)
            {
                _constantValuedVariablesAndMembers[valueHolder.GetFullName()] = null;
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
